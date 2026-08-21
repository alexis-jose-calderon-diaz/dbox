using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class ActivityConcurrencyTests
{
    [Fact]
    public async Task ActivityMetadataStartsAtOneAndConditionalUpdatesRejectStaleVersions()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        var add = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Concurrency\",\"description\":\"Details\",\"status\":\"pending\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Initial\",\"impact\":\"Impact\",\"effort\":\"low\"}");
        using var addDocument = JsonDocument.Parse(add.Output);
        var createdAt = addDocument.RootElement.GetProperty("created_at").GetString();

        var missingVersion = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"result\":\"Should not persist\"}");
        var invalidVersion = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"result\":\"Should not persist\",\"version\":0}");
        var generatedVersion = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"result\":\"Should not persist\",\"version\":\"1\"}");

        await Task.Delay(TimeSpan.FromMilliseconds(1100));
        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"result\":\"Updated\",\"version\":1}");
        using var updateDocument = JsonDocument.Parse(update.Output);
        var stale = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"result\":\"Stale overwrite\",\"version\":1}");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1");
        using var getDocument = JsonDocument.Parse(get.Output);

        Assert.Equal(1, addDocument.RootElement.GetProperty("version").GetInt64());
        Assert.Equal(
            createdAt,
            addDocument.RootElement.GetProperty("updated_at").GetString());
        Assert.Equal(2, updateDocument.RootElement.GetProperty("version").GetInt64());
        Assert.NotEqual(
            createdAt,
            updateDocument.RootElement.GetProperty("updated_at").GetString());
        Assert.Equal(createdAt, getDocument.RootElement.GetProperty("created_at").GetString());
        Assert.Equal("Updated", getDocument.RootElement.GetProperty("result").GetString());
        Assert.Equal(2, getDocument.RootElement.GetProperty("version").GetInt64());
        Assert.Equal(2, missingVersion.ExitCode);
        Assert.Equal(2, invalidVersion.ExitCode);
        Assert.Equal(2, generatedVersion.ExitCode);
        Assert.Equal(3, stale.ExitCode);
        Assert.Empty(stale.Output);
        Assert.Contains("conflict_error", stale.Error);
    }
}
