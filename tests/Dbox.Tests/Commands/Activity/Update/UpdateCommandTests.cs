using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Update;

public sealed class UpdateCommandTests
{
    [Fact]
    public async Task UpdateSupportsPartialJsonAndOptionInputs()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", "Investigate");
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
        using var getDocument = JsonDocument.Parse(get.Output);

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
    }
}
