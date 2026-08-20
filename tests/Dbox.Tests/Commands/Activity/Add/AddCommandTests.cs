using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Add;

public sealed class AddCommandTests
{
    [Fact]
    public async Task ValidationHappensBeforePersistenceAndUsesStderr()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var invalid = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--type",
            "invalid",
            "--title",
            "",
            "--output",
            "json");
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(2, invalid.ExitCode);
        Assert.Empty(invalid.Output);
        Assert.Contains("validation_error", invalid.Error);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task EnumCasingAndTitleLengthAreValidatedBeforePersistence()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var wrongCase = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "Research", "--title", "Valid");
        var tooLong = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", new string('x', 201));
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(2, wrongCase.ExitCode);
        Assert.Equal(2, tooLong.ExitCode);
        Assert.Contains("must be one of", wrongCase.Error);
        Assert.Contains("at most 200", tooLong.Error);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task MixedInputAndUnknownJsonPropertiesAreRejected()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var mixed = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\"}",
            "--type",
            "research");
        var unknown = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\",\"unknown\":true}");

        Assert.Equal(2, mixed.ExitCode);
        Assert.Equal(2, unknown.ExitCode);
        Assert.Contains("Validation error:", mixed.Error);
        Assert.Contains("Unknown property", unknown.Error);
    }
}
