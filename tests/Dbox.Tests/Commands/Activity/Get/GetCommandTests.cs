using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Get;

public sealed class GetCommandTests
{
    [Fact]
    public async Task MissingActivityUsesTheJsonErrorAndExitCode()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var result = await TestProject.RunAsync(project.Root, "activity", "get", "7");

        Assert.Equal(3, result.ExitCode);
        Assert.Contains("resource_not_found", result.Error);
        Assert.Contains("Activity 7 not found.", result.Error);
    }

    [Fact]
    public async Task JsonOutputPreservesControlCharactersInActivityValues()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"line\\nbreak\\tvalue\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"low\"}");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1");

        Assert.Equal(0, add.ExitCode);
        Assert.Equal(0, get.ExitCode);
        Assert.Contains("\\n", get.Output);
        Assert.Contains("\\t", get.Output);
    }
}
