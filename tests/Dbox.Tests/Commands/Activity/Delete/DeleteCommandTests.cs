using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Delete;

public sealed class DeleteCommandTests
{
    [Fact]
    public async Task DeleteRemovesAnActivityAndReportsMissingResources()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Delete me\"}");

        var deleted = await TestProject.RunAsync(project.Root, "activity", "delete", "1");
        var missing = await TestProject.RunAsync(project.Root, "activity", "get", "1");

        Assert.Equal(0, deleted.ExitCode);
        Assert.Contains("\"deleted\": true", deleted.Output);
        Assert.Equal(3, missing.ExitCode);
        Assert.Contains("resource_not_found", missing.Error);
    }
}
