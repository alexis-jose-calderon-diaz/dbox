using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Get;

public sealed class GetCommandTests
{
    [Fact]
    public async Task MissingActivityUsesTheDocumentedMessageAndExitCode()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var result = await TestProject.RunAsync(project.Root, "activity", "get", "7");

        Assert.Equal(3, result.ExitCode);
        Assert.Equal("Activity 7 not found.\n", result.Error);
    }

    [Fact]
    public async Task TextOutputEscapesControlCharactersInActivityValues()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", "line\nbreak\tvalue");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1");

        Assert.Equal(0, add.ExitCode);
        Assert.Equal(0, get.ExitCode);
        Assert.Contains("TITLE: line\\nbreak\\tvalue", get.Output);
        Assert.DoesNotContain("TITLE: line\nbreak", get.Output);
    }
}
