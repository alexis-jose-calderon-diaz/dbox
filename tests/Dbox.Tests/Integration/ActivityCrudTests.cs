using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class ActivityCrudTests
{
    [Fact]
    public async Task CrudSupportsJsonFiltersPartialUpdatesAndDeletion()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var firstAdd = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--type",
            "research",
            "--title",
            "Investigate",
            "--output",
            "json");
        var secondAdd = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"implementation\",\"title\":\"Build\",\"description\":\"Details\",\"status\":\"pending\"}",
            "--output",
            "json");
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");
        var filteredList = await TestProject.RunAsync(project.Root, "activity", "list", "--type", "implementation", "--status", "pending", "--output", "json");
        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"description\":null,\"status\":\"completed\"}",
            "--output",
            "json");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1", "--output", "json");
        var emptyUpdate = await TestProject.RunAsync(project.Root, "activity", "update", "1", "--output", "json");
        var optionUpdate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "2",
            "--status",
            "in_progress",
            "--output",
            "json");
        var generatedUpdate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"id\":99}",
            "--output",
            "json");
        var deleted = await TestProject.RunAsync(project.Root, "activity", "delete", "1", "--output", "json");
        var missing = await TestProject.RunAsync(project.Root, "activity", "get", "1", "--output", "json");
        using var listDocument = JsonDocument.Parse(list.Output);
        using var filteredDocument = JsonDocument.Parse(filteredList.Output);
        using var firstAddDocument = JsonDocument.Parse(firstAdd.Output);
        using var getDocument = JsonDocument.Parse(get.Output);

        Assert.Equal(0, firstAdd.ExitCode);
        Assert.Contains("\"status\": \"completed\"", firstAdd.Output);
        Assert.Matches(
            "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}Z$",
            firstAddDocument.RootElement.GetProperty("created_at").GetString());
        Assert.Equal(0, secondAdd.ExitCode);
        Assert.Contains("\"status\": \"pending\"", secondAdd.Output);
        Assert.Equal(0, list.ExitCode);
        Assert.Equal(2, listDocument.RootElement.GetArrayLength());
        Assert.Equal(2, listDocument.RootElement[0].GetProperty("id").GetInt64());
        Assert.Equal(1, listDocument.RootElement[1].GetProperty("id").GetInt64());
        Assert.Equal(1, filteredDocument.RootElement.GetArrayLength());
        Assert.Equal("Build", filteredDocument.RootElement[0].GetProperty("title").GetString());
        Assert.Equal(0, update.ExitCode);
        Assert.Contains("\"description\": null", update.Output);
        Assert.Equal(0, get.ExitCode);
        Assert.Equal("Investigate", getDocument.RootElement.GetProperty("title").GetString());
        Assert.Equal("completed", getDocument.RootElement.GetProperty("status").GetString());
        Assert.True(getDocument.RootElement.GetProperty("description").ValueKind == JsonValueKind.Null);
        Assert.Equal(2, emptyUpdate.ExitCode);
        Assert.Equal(0, optionUpdate.ExitCode);
        Assert.Contains("\"status\": \"in_progress\"", optionUpdate.Output);
        Assert.Equal(2, generatedUpdate.ExitCode);
        Assert.Equal(0, deleted.ExitCode);
        Assert.Contains("\"deleted\": true", deleted.Output);
        Assert.Equal(3, missing.ExitCode);
        Assert.Contains("resource_not_found", missing.Error);
    }
}
