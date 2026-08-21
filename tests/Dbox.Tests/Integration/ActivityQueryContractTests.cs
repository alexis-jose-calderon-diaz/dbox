using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class ActivityQueryContractTests
{
    [Fact]
    public async Task ListAndCountApplyAllFiltersWithTheSameSemantics()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var first = await AddAsync(
            project.Root,
            "Database Query",
            "Database Sensitive Details",
            "research",
            "pending",
            "openspec",
            "backend",
            "low");
        await AddAsync(
            project.Root,
            "Other Work",
            "Frontend details",
            "feature",
            "completed",
            "manual",
            "frontend",
            "high");
        await AddAsync(
            project.Root,
            "Another QUERY",
            "The database result",
            "research",
            "completed",
            "research",
            "backend",
            "medium");

        using var firstDocument = JsonDocument.Parse(first.Output);
        var firstCreatedAt = firstDocument.RootElement.GetProperty("created_at").GetString()!;
        var filters = new[]
        {
            ("{\"type\":\"research\"}", 2),
            ("{\"status\":\"completed\"}", 2),
            ("{\"area\":\"backend\"}", 2),
            ("{\"source\":\"openspec\"}", 1),
            ("{\"effort\":\"low\"}", 1),
            ("{\"title\":\"query\"}", 2),
            ("{\"description\":\"DATABASE\"}", 2),
            ("{\"created_from\":\"2000-01-01T00:00:00Z\",\"created_to\":\"2100-01-01T00:00:00Z\"}", 3),
            ($"{{\"created_from\":\"{firstCreatedAt}\",\"created_to\":\"2100-01-01T00:00:00Z\"}}", 3)
        };

        foreach (var (filter, expected) in filters)
        {
            var list = await TestProject.RunAsync(project.Root, "activity", "list", "--json", filter);
            var count = await TestProject.RunAsync(project.Root, "activity", "count", "--json", filter);
            using var listDocument = JsonDocument.Parse(list.Output);
            using var countDocument = JsonDocument.Parse(count.Output);

            Assert.Equal(0, list.ExitCode);
            Assert.Equal(0, count.ExitCode);
            Assert.Equal(expected, listDocument.RootElement.GetProperty("items").GetArrayLength());
            Assert.Equal(expected, listDocument.RootElement.GetProperty("pagination").GetProperty("total").GetInt32());
            Assert.Equal(expected, countDocument.RootElement.GetProperty("count").GetInt32());
        }

        const string combinedFilter = "{\"type\":\"research\",\"status\":\"pending\",\"area\":\"backend\",\"source\":\"openspec\",\"effort\":\"low\",\"created_from\":\"2000-01-01T00:00:00Z\",\"created_to\":\"2100-01-01T00:00:00Z\",\"title\":\"DATABASE\",\"description\":\"sensitive\"}";
        var combinedList = await TestProject.RunAsync(project.Root, "activity", "list", "--json", combinedFilter);
        var combinedCount = await TestProject.RunAsync(project.Root, "activity", "count", "--json", combinedFilter);
        using var combinedListDocument = JsonDocument.Parse(combinedList.Output);
        using var combinedCountDocument = JsonDocument.Parse(combinedCount.Output);

        Assert.Equal(0, combinedList.ExitCode);
        Assert.Equal(0, combinedCount.ExitCode);
        Assert.Equal(1, combinedListDocument.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(1, combinedCountDocument.RootElement.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task InvalidFiltersAndPaginationReturnDeterministicValidationErrors()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var invalidFilters = new[]
        {
            ("list", "{\"unknown\":true}"),
            ("count", "{\"status\":\"done\"}"),
            ("list", "{\"created_from\":\"not-a-date\"}"),
            ("count", "{\"created_from\":\"2027-01-01T00:00:00Z\",\"created_to\":\"2026-01-01T00:00:00Z\"}"),
            ("list", "{\"title\":\"   \"}"),
            ("count", "{\"effort\":\"urgent\"}")
        };

        foreach (var (command, filter) in invalidFilters)
        {
            var result = await TestProject.RunAsync(
                project.Root,
                "activity",
                command,
                "--json",
                filter);
            AssertValidationError(result);
        }

        var negativeSkip = await TestProject.RunAsync(project.Root, "activity", "list", "--skip", "-1");
        var negativeTake = await TestProject.RunAsync(project.Root, "activity", "list", "--take", "-1");
        var allWithTake = await TestProject.RunAsync(
            project.Root,
            "activity",
            "list",
            "--all",
            "--take",
            "100");

        AssertValidationError(negativeSkip);
        AssertValidationError(negativeTake);
        AssertValidationError(
            allWithTake,
            "Options '--all' and '--take' cannot be used together.");
    }

    [Fact]
    public async Task ListUsesTheEnvelopeDefaultLimitAndUnboundedMode()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        for (var index = 0; index < 101; index++)
        {
            var result = await AddAsync(
                project.Root,
                $"Activity {index:D3}",
                "Details",
                "research",
                "completed",
                "manual",
                "backend",
                "low");
            Assert.Equal(0, result.ExitCode);
        }

        var defaultPage = await TestProject.RunAsync(project.Root, "activity", "list");
        var secondPage = await TestProject.RunAsync(
            project.Root,
            "activity",
            "list",
            "--skip",
            "100",
            "--take",
            "10");
        var unbounded = await TestProject.RunAsync(project.Root, "activity", "list", "--all");
        var empty = await TestProject.RunAsync(
            project.Root,
            "activity",
            "list",
            "--json",
            "{\"title\":\"does not exist\"}");

        using var defaultDocument = JsonDocument.Parse(defaultPage.Output);
        using var secondDocument = JsonDocument.Parse(secondPage.Output);
        using var unboundedDocument = JsonDocument.Parse(unbounded.Output);
        using var emptyDocument = JsonDocument.Parse(empty.Output);
        var defaultPagination = defaultDocument.RootElement.GetProperty("pagination");
        var secondPagination = secondDocument.RootElement.GetProperty("pagination");
        var unboundedPagination = unboundedDocument.RootElement.GetProperty("pagination");
        var emptyPagination = emptyDocument.RootElement.GetProperty("pagination");

        Assert.Equal(100, defaultDocument.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(0, defaultPagination.GetProperty("skip").GetInt32());
        Assert.Equal(100, defaultPagination.GetProperty("take").GetInt32());
        Assert.Equal(101, defaultPagination.GetProperty("total").GetInt32());
        Assert.True(defaultPagination.GetProperty("has_more").GetBoolean());
        Assert.Equal(1, secondDocument.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(100, secondPagination.GetProperty("skip").GetInt32());
        Assert.Equal(10, secondPagination.GetProperty("take").GetInt32());
        Assert.Equal(101, secondPagination.GetProperty("total").GetInt32());
        Assert.False(secondPagination.GetProperty("has_more").GetBoolean());
        Assert.Equal(101, unboundedDocument.RootElement.GetProperty("items").GetArrayLength());
        Assert.Equal(JsonValueKind.Null, unboundedPagination.GetProperty("take").ValueKind);
        Assert.Equal(101, unboundedPagination.GetProperty("total").GetInt32());
        Assert.False(unboundedPagination.GetProperty("has_more").GetBoolean());
        Assert.Empty(emptyDocument.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(0, emptyPagination.GetProperty("total").GetInt32());
        Assert.False(emptyPagination.GetProperty("has_more").GetBoolean());
    }

    [Fact]
    public async Task HelpDescribesJsonFilesFiltersAndAll()
    {
        using var project = new TestProject();
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--help");
        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--help");
        var count = await TestProject.RunAsync(project.Root, "activity", "count", "--help");
        var update = await TestProject.RunAsync(project.Root, "activity", "update", "--help");

        Assert.Contains("--json", list.Output);
        Assert.Contains("--json-file", list.Output);
        Assert.Contains("--skip", list.Output);
        Assert.Contains("--take", list.Output);
        Assert.Contains("--all", list.Output);
        Assert.Contains("--json-file", add.Output);
        Assert.Contains("--json-file", count.Output);
        Assert.Contains("--json-file", update.Output);
    }

    private static async Task<CliResult> AddAsync(
        string root,
        string title,
        string description,
        string type,
        string status,
        string source,
        string area,
        string effort)
    {
        return await TestProject.RunAsync(
            root,
            "activity",
            "add",
            "--json",
            ActivityJson(title, description, type, status, source, area, effort));
    }

    private static string ActivityJson(
        string title,
        string description,
        string type,
        string status,
        string source,
        string area,
        string effort) =>
        $"{{\"type\":\"{type}\",\"title\":\"{title}\",\"description\":\"{description}\",\"status\":\"{status}\",\"source\":\"{source}\",\"area\":\"{area}\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"{effort}\"}}";

    private static void AssertValidationError(CliResult result, string? message = null)
    {
        using var document = JsonDocument.Parse(result.Error);
        var error = document.RootElement.GetProperty("error");
        Assert.Equal(2, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Equal("validation_error", error.GetProperty("code").GetString());
        if (message is not null)
        {
            Assert.Equal(message, error.GetProperty("message").GetString());
            Assert.Equal(message, error.GetProperty("details")[0].GetProperty("message").GetString());
        }
    }
}
