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
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Investigate\",\"description\":\"Details\",\"status\":\"pending\",\"source\":\"openspec\",\"area\":\"backend\",\"result\":\"Initial result\",\"impact\":\"Initial impact\",\"effort\":\"medium\",\"reference\":\"issue #1\",\"metadata\":{\"openspec\":\"activity-contract\"}}");
        await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"implementation\",\"title\":\"Build\",\"description\":\"Details\",\"status\":\"pending\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Build result\",\"impact\":\"Build impact\",\"effort\":\"high\"}");

        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"description\":\"Updated details\",\"result\":\"Updated result\",\"status\":\"completed\",\"reference\":null,\"metadata\":null,\"version\":1}");
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
        Assert.Contains("\"description\": \"Updated details\"", update.Output);
        Assert.Equal(0, get.ExitCode);
        Assert.Equal("Investigate", getDocument.RootElement.GetProperty("title").GetString());
        Assert.Equal("completed", getDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal("Updated details", getDocument.RootElement.GetProperty("description").GetString());
        Assert.Equal("Updated result", getDocument.RootElement.GetProperty("result").GetString());
        Assert.Equal(JsonValueKind.Null, getDocument.RootElement.GetProperty("reference").ValueKind);
        Assert.Equal(JsonValueKind.Null, getDocument.RootElement.GetProperty("metadata").ValueKind);
        Assert.Equal(2, emptyUpdate.ExitCode);
        Assert.Equal(2, optionUpdate.ExitCode);
        Assert.Equal(2, generatedUpdate.ExitCode);
    }
}
