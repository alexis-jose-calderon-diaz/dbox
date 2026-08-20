using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Update;

public sealed class UpdateCommandTests
{
    [Fact]
    public async Task UpdateRequiresJsonAndSupportsPartialUpdates()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Investigate\"}");
        await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"implementation\",\"title\":\"Build\",\"description\":\"Details\",\"status\":\"pending\"}");

        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"description\":null,\"status\":\"completed\"}");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1");
        var emptyUpdate = await TestProject.RunAsync(project.Root, "activity", "update", "1");
        var optionUpdate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "2",
            "--status",
            "in_progress");
        var generatedUpdate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"id\":99}");
        using var getDocument = JsonDocument.Parse(get.Output);

        Assert.Equal(0, update.ExitCode);
        Assert.Contains("\"description\": null", update.Output);
        Assert.Equal(0, get.ExitCode);
        Assert.Equal("Investigate", getDocument.RootElement.GetProperty("title").GetString());
        Assert.Equal("completed", getDocument.RootElement.GetProperty("status").GetString());
        Assert.True(getDocument.RootElement.GetProperty("description").ValueKind == JsonValueKind.Null);
        Assert.Equal(2, emptyUpdate.ExitCode);
        Assert.Equal(2, optionUpdate.ExitCode);
        Assert.Equal(2, generatedUpdate.ExitCode);
    }
}
