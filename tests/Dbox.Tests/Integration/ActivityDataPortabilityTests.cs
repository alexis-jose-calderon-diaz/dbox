using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class ActivityDataPortabilityTests
{
    [Fact]
    public async Task ActivityHelpExposesPortableCommandsAndTheirOptions()
    {
        using var project = new TestProject();
        var activity = await TestProject.RunAsync(project.Root, "activity", "--help");
        var export = await TestProject.RunAsync(project.Root, "activity", "export", "--help");
        var import = await TestProject.RunAsync(project.Root, "activity", "import", "--help");

        Assert.Equal(0, activity.ExitCode);
        Assert.Equal(0, export.ExitCode);
        Assert.Equal(0, import.ExitCode);
        Assert.Contains("export", activity.Output);
        Assert.Contains("import", activity.Output);
        Assert.Contains("--format", export.Output);
        Assert.Contains("--file", import.Output);
        Assert.Contains("--format", import.Output);
    }

    [Fact]
    public async Task ExportProducesCompleteRecordsInStableJsonAndJsonlFormats()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            ActivityJson("First", "completed", "manual", "backend", "low"));
        await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            ActivityJson("Second", "pending", "openspec", "frontend", "medium"));

        var json = await TestProject.RunAsync(project.Root, "activity", "export");
        var jsonl = await TestProject.RunAsync(project.Root, "activity", "export", "--format", "jsonl");
        using var jsonDocument = JsonDocument.Parse(json.Output);
        var lines = jsonl.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(0, json.ExitCode);
        Assert.Equal(0, jsonl.ExitCode);
        Assert.Equal(2, jsonDocument.RootElement.GetArrayLength());
        Assert.Equal(1, jsonDocument.RootElement[0].GetProperty("id").GetInt64());
        Assert.Equal(2, jsonDocument.RootElement[1].GetProperty("id").GetInt64());
        AssertCompleteRecord(jsonDocument.RootElement[0]);
        Assert.Equal(2, lines.Length);
        Assert.DoesNotContain("\n\n", jsonl.Output);
        using var firstLine = JsonDocument.Parse(lines[0]);
        using var secondLine = JsonDocument.Parse(lines[1]);
        Assert.Equal(1, firstLine.RootElement.GetProperty("id").GetInt64());
        Assert.Equal(2, secondLine.RootElement.GetProperty("id").GetInt64());

        var emptyProject = new TestProject();
        try
        {
            await TestProject.RunAsync(emptyProject.Root, "init");
            var emptyJsonl = await TestProject.RunAsync(
                emptyProject.Root,
                "activity",
                "export",
                "--format",
                "jsonl");
            var emptyJson = await TestProject.RunAsync(
                emptyProject.Root,
                "activity",
                "export",
                "--format",
                "json");

            Assert.Equal(0, emptyJsonl.ExitCode);
            Assert.Empty(emptyJsonl.Output);
            Assert.Equal(0, emptyJson.ExitCode);
            Assert.Equal("[]", emptyJson.Output.Trim());
        }
        finally
        {
            emptyProject.Dispose();
        }

        var unsupported = await TestProject.RunAsync(project.Root, "activity", "export", "--format", "csv");
        AssertValidationError(unsupported);
    }

    [Fact]
    public async Task ImportPreservesAllFieldsForJsonAndJsonl()
    {
        using var source = new TestProject();
        await TestProject.RunAsync(source.Root, "init");
        var add = await TestProject.RunAsync(
            source.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Portable\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"research\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\",\"reference\":\"issue-7\",\"metadata\":{\"source\":\"test\",\"items\":[1,2]}}");
        using var addDocument = JsonDocument.Parse(add.Output);
        var update = await TestProject.RunAsync(
            source.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"result\":\"Updated result\",\"version\":1}");
        Assert.Equal(0, update.ExitCode);

        var jsonl = await TestProject.RunAsync(source.Root, "activity", "export", "--format", "jsonl");
        var jsonPath = Path.Combine(source.Root, "activities.json");
        File.WriteAllText(jsonPath, (await TestProject.RunAsync(source.Root, "activity", "export")).Output);

        using var target = new TestProject();
        await TestProject.RunAsync(target.Root, "init");
        var jsonImport = await TestProject.RunAsync(
            target.Root,
            "activity",
            "import",
            "--file",
            jsonPath,
            "--format",
            "json");
        var importedJson = await TestProject.RunAsync(target.Root, "activity", "get", "1");
        using var importedJsonDocument = JsonDocument.Parse(importedJson.Output);

        Assert.Equal(0, jsonImport.ExitCode);
        Assert.Contains("\"imported\": 1", jsonImport.Output);
        Assert.Contains("\"format\": \"json\"", jsonImport.Output);
        Assert.Equal(0, importedJson.ExitCode);
        Assert.Equal("Updated result", importedJsonDocument.RootElement.GetProperty("result").GetString());
        Assert.Equal(2, importedJsonDocument.RootElement.GetProperty("version").GetInt64());
        Assert.Equal("issue-7", importedJsonDocument.RootElement.GetProperty("reference").GetString());
        Assert.Equal("test", importedJsonDocument.RootElement.GetProperty("metadata").GetProperty("source").GetString());
        AssertCompleteRecord(importedJsonDocument.RootElement);

        using var jsonlTarget = new TestProject();
        await TestProject.RunAsync(jsonlTarget.Root, "init");
        var jsonlPath = Path.Combine(source.Root, "activities.jsonl");
        File.WriteAllText(jsonlPath, jsonl.Output);
        var jsonlImport = await TestProject.RunAsync(
            jsonlTarget.Root,
            "activity",
            "import",
            "--file",
            jsonlPath,
            "--format",
            "jsonl");
        var importedJsonl = await TestProject.RunAsync(jsonlTarget.Root, "activity", "get", "1");

        Assert.Equal(0, jsonlImport.ExitCode);
        Assert.Contains("\"format\": \"jsonl\"", jsonlImport.Output);
        Assert.Equal(0, importedJsonl.ExitCode);
        Assert.Contains("Updated result", importedJsonl.Output);
    }

    [Fact]
    public async Task ImportRejectsInvalidDataUnreadableFilesAndIdConflictsAtomically()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var incompletePath = Path.Combine(project.Root, "incomplete.json");
        File.WriteAllText(incompletePath, "[{\"id\":1}]");
        var incomplete = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            incompletePath,
            "--format",
            "json");
        AssertValidationError(incomplete);
        Assert.Equal(0, await ReadCountAsync(project.Root));

        var unknownPath = Path.Combine(project.Root, "unknown.json");
        File.WriteAllText(unknownPath, "[{\"unknown\":true}]");
        var unknown = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            unknownPath,
            "--format",
            "json");
        AssertValidationError(unknown);

        var duplicatePath = Path.Combine(project.Root, "duplicate.json");
        var record = PortableJson(10, "Duplicate");
        File.WriteAllText(duplicatePath, $"[{record},{record}]");
        var duplicate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            duplicatePath,
            "--format",
            "json");
        AssertConflictError(duplicate);
        Assert.Equal(0, await ReadCountAsync(project.Root));

        var unreadable = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            Path.Combine(project.Root, "missing.json"),
            "--format",
            "json");
        using var unreadableDocument = JsonDocument.Parse(unreadable.Error);
        Assert.Equal(4, unreadable.ExitCode);
        Assert.Empty(unreadable.Output);
        Assert.Equal("io_error", unreadableDocument.RootElement.GetProperty("error").GetProperty("code").GetString());

        var missingOption = await TestProject.RunAsync(project.Root, "activity", "import", "--format", "json");
        var missingFormat = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            duplicatePath);
        AssertValidationError(missingOption);
        AssertValidationError(missingFormat);
    }

    [Fact]
    public async Task ImportRejectsExistingIdsAndBlankJsonlLinesWithoutPartialWrites()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            ActivityJson("Existing", "pending", "manual", "backend", "low"));
        var conflictPath = Path.Combine(project.Root, "conflict.json");
        File.WriteAllText(conflictPath, $"[{PortableJson(1, "Existing" )},{PortableJson(2, "New")}]" );
        var conflict = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            conflictPath,
            "--format",
            "json");
        var countAfterConflict = await ReadCountAsync(project.Root);

        var blankLinePath = Path.Combine(project.Root, "blank.jsonl");
        File.WriteAllText(blankLinePath, $"{PortableJson(3, "Line one")}\n\n{PortableJson(4, "Line two")}\n");
        var blankLine = await TestProject.RunAsync(
            project.Root,
            "activity",
            "import",
            "--file",
            blankLinePath,
            "--format",
            "jsonl");

        AssertConflictError(conflict);
        Assert.Equal(1, countAfterConflict);
        AssertValidationError(blankLine);
        Assert.Equal(1, await ReadCountAsync(project.Root));
    }

    private static async Task<int> ReadCountAsync(string root)
    {
        var count = await TestProject.RunAsync(root, "activity", "count");
        using var document = JsonDocument.Parse(count.Output);
        return document.RootElement.GetProperty("count").GetInt32();
    }

    private static string ActivityJson(
        string title,
        string status,
        string source,
        string area,
        string effort) =>
        $"{{\"type\":\"research\",\"title\":\"{title}\",\"description\":\"Details\",\"status\":\"{status}\",\"source\":\"{source}\",\"area\":\"{area}\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"{effort}\"}}";

    private static string PortableJson(long id, string title) =>
        $"{{\"id\":{id},\"created_at\":\"2026-01-01T00:00:00Z\",\"updated_at\":\"2026-01-01T00:00:01Z\",\"version\":2,\"type\":\"research\",\"title\":\"{title}\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"low\",\"reference\":null,\"metadata\":null}}";

    private static void AssertCompleteRecord(JsonElement record)
    {
        Assert.True(record.TryGetProperty("id", out _));
        Assert.True(record.TryGetProperty("created_at", out _));
        Assert.True(record.TryGetProperty("updated_at", out _));
        Assert.True(record.TryGetProperty("version", out _));
        Assert.True(record.TryGetProperty("type", out _));
        Assert.True(record.TryGetProperty("title", out _));
        Assert.True(record.TryGetProperty("description", out _));
        Assert.True(record.TryGetProperty("status", out _));
        Assert.True(record.TryGetProperty("source", out _));
        Assert.True(record.TryGetProperty("area", out _));
        Assert.True(record.TryGetProperty("result", out _));
        Assert.True(record.TryGetProperty("impact", out _));
        Assert.True(record.TryGetProperty("effort", out _));
        Assert.True(record.TryGetProperty("reference", out _));
        Assert.True(record.TryGetProperty("metadata", out _));
    }

    private static void AssertValidationError(CliResult result)
    {
        using var document = JsonDocument.Parse(result.Error);
        Assert.Equal(2, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Equal("validation_error", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    private static void AssertConflictError(CliResult result)
    {
        using var document = JsonDocument.Parse(result.Error);
        Assert.Equal(3, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Equal("conflict_error", document.RootElement.GetProperty("error").GetProperty("code").GetString());
    }
}
