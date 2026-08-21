using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class ActivityJsonInputTests
{
    [Fact]
    public async Task ActivityPayloadsCanBeReadFromFilesAndStandardInput()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var createPayload = ActivityJson("File activity", "completed", "manual", "backend", "low");
        var createPath = Path.Combine(project.Root, "create.json");
        File.WriteAllText(createPath, createPayload);
        var fromFile = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json-file",
            createPath);
        var fromStdin = await TestProject.RunWithInputAsync(
            project.Root,
            ActivityJson("Stdin activity", "pending", "openspec", "backend", "medium"),
            "activity",
            "add",
            "--json-file",
            "-");

        var updatePath = Path.Combine(project.Root, "update.json");
        File.WriteAllText(updatePath, "{\"title\":\"Updated from file\",\"version\":1}");
        var updateFromFile = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json-file",
            updatePath);
        var updateFromStdin = await TestProject.RunWithInputAsync(
            project.Root,
            "{\"status\":\"completed\",\"version\":1}",
            "activity",
            "update",
            "2",
            "--json-file",
            "-");

        var filterPath = Path.Combine(project.Root, "filter.json");
        File.WriteAllText(filterPath, "{\"source\":\"openspec\"}");
        var listFromFile = await TestProject.RunAsync(
            project.Root,
            "activity",
            "list",
            "--json-file",
            filterPath);
        var countFromStdin = await TestProject.RunWithInputAsync(
            project.Root,
            "{\"status\":\"completed\"}",
            "activity",
            "count",
            "--json-file",
            "-");

        Assert.Equal(0, fromFile.ExitCode);
        Assert.Equal(0, fromStdin.ExitCode);
        Assert.Contains("File activity", fromFile.Output);
        Assert.Contains("Stdin activity", fromStdin.Output);
        Assert.Equal(0, updateFromFile.ExitCode);
        Assert.Equal(0, updateFromStdin.ExitCode);
        Assert.Contains("Updated from file", updateFromFile.Output);
        Assert.Contains("\"status\": \"completed\"", updateFromStdin.Output);
        Assert.Equal(0, listFromFile.ExitCode);
        Assert.Contains("Stdin activity", listFromFile.Output);
        Assert.Equal(0, countFromStdin.ExitCode);
        using var countDocument = JsonDocument.Parse(countFromStdin.Output);
        Assert.Equal(2, countDocument.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task JsonPayloadSourcesAreMutuallyExclusiveForEveryPayloadCommand()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var path = Path.Combine(project.Root, "payload.json");
        File.WriteAllText(path, "{}");
        var payload = ActivityJson("Conflict", "completed", "manual", "backend", "low");

        var add = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            payload,
            "--json-file",
            path);
        var list = await TestProject.RunAsync(
            project.Root,
            "activity",
            "list",
            "--json",
            "{}",
            "--json-file",
            path);
        var count = await TestProject.RunAsync(
            project.Root,
            "activity",
            "count",
            "--json",
            "{}",
            "--json-file",
            path);
        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"status\":\"completed\"}",
            "--json-file",
            path);

        AssertSourceError(add, ActivityInputParserMessage.Conflict);
        AssertSourceError(list, ActivityInputParserMessage.Conflict);
        AssertSourceError(count, ActivityInputParserMessage.Conflict);
        AssertSourceError(update, ActivityInputParserMessage.Conflict);
    }

    [Fact]
    public async Task UnreadableJsonFilesUseTheStableValidationMessage()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var missingPath = Path.Combine(project.Root, "does-not-exist.json");

        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--json-file", missingPath);
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--json-file", missingPath);
        var count = await TestProject.RunAsync(project.Root, "activity", "count", "--json-file", missingPath);
        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json-file",
            missingPath);

        AssertSourceError(add, ActivityInputParserMessage.Unreadable);
        AssertSourceError(list, ActivityInputParserMessage.Unreadable);
        AssertSourceError(count, ActivityInputParserMessage.Unreadable);
        AssertSourceError(update, ActivityInputParserMessage.Unreadable);
    }

    [Fact]
    public async Task InvalidJsonUsesTheStableValidationMessageFromEverySource()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var invalidPath = Path.Combine(project.Root, "invalid.json");
        File.WriteAllText(invalidPath, "{\"type\":");

        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{");
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--json-file", invalidPath);
        var count = await TestProject.RunWithInputAsync(
            project.Root,
            "[]",
            "activity",
            "count",
            "--json-file",
            "-");
        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json-file",
            invalidPath);

        AssertSourceError(add, ActivityInputParserMessage.InvalidJson);
        AssertSourceError(list, ActivityInputParserMessage.InvalidJson);
        AssertSourceError(count, ActivityInputParserMessage.InvalidJson);
        AssertSourceError(update, ActivityInputParserMessage.InvalidJson);
    }

    private static void AssertSourceError(CliResult result, string message)
    {
        using var document = JsonDocument.Parse(result.Error);
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(2, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Equal("validation_error", error.GetProperty("code").GetString());
        Assert.Equal(message, error.GetProperty("message").GetString());
        Assert.Equal(message, error.GetProperty("details")[0].GetProperty("message").GetString());
    }

    private static string ActivityJson(
        string title,
        string status,
        string source,
        string area,
        string effort) =>
        $"{{\"type\":\"research\",\"title\":\"{title}\",\"description\":\"Activity details\",\"status\":\"{status}\",\"source\":\"{source}\",\"area\":\"{area}\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"{effort}\"}}";

    private static class ActivityInputParserMessage
    {
        public const string Conflict = "Specify either '--json' or '--json-file', not both.";
        public const string Unreadable = "Unable to read JSON input.";
        public const string InvalidJson = "JSON input must be a valid JSON object.";
    }
}
