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
            "--json",
            "{\"type\":\"invalid\",\"title\":\"\"}");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");

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

        var wrongCase = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"Research\",\"title\":\"Valid\"}");
        var tooLong = await TestProject.RunAsync(project.Root, "activity", "add", "--json", $"{{\"type\":\"research\",\"title\":\"{new string('x', 201)}\"}}");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(2, wrongCase.ExitCode);
        Assert.Equal(2, tooLong.ExitCode);
        Assert.Contains("must be one of", wrongCase.Error);
        Assert.Contains("at most 200", tooLong.Error);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task MissingPayloadFieldOptionsAndUnknownJsonPropertiesAreRejected()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var missing = await TestProject.RunAsync(project.Root, "activity", "add");
        var fieldOption = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--type",
            "research");
        var unknown = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\",\"unknown\":true}");

        Assert.Equal(2, missing.ExitCode);
        Assert.Equal(2, fieldOption.ExitCode);
        Assert.Equal(2, unknown.ExitCode);
        Assert.Contains("validation_error", missing.Error);
        Assert.Contains("validation_error", fieldOption.Error);
        Assert.Contains("Unknown property", unknown.Error);
    }
}
