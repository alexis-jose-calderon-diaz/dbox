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
            "{\"type\":\"research\",\"title\":\"\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\"}");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(2, invalid.ExitCode);
        Assert.Empty(invalid.Output);
        Assert.Contains("validation_error", invalid.Error);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task ControlledValueCasingAndTitleLengthAreValidatedBeforePersistence()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var wrongCase = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"Research\",\"title\":\"Valid\",\"description\":\"Details\",\"status\":\"Completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\"}");
        var invalidEffort = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Valid\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"Medium\"}");
        var tooLong = await TestProject.RunAsync(project.Root, "activity", "add", "--json", $"{{\"type\":\"research\",\"title\":\"{new string('x', 201)}\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\"}}");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(2, wrongCase.ExitCode);
        Assert.Equal(2, invalidEffort.ExitCode);
        Assert.Equal(2, tooLong.ExitCode);
        Assert.Contains("must be one of", wrongCase.Error);
        Assert.Contains("must be one of", invalidEffort.Error);
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
            "{\"type\":\"research\",\"title\":\"Test\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\",\"unknown\":true}");

        Assert.Equal(2, missing.ExitCode);
        Assert.Equal(2, fieldOption.ExitCode);
        Assert.Equal(2, unknown.ExitCode);
        Assert.Contains("validation_error", missing.Error);
        Assert.Contains("validation_error", fieldOption.Error);
        Assert.Contains("Unknown property", unknown.Error);
    }

    [Fact]
    public async Task RequiredFieldsAndMetadataAreValidatedBeforePersistence()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var missing = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\"}");
        var blankDescription = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\",\"description\":\" \",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\"}");
        var invalidMetadata = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"medium\",\"metadata\":[]}");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(2, missing.ExitCode);
        Assert.Equal(2, blankDescription.ExitCode);
        Assert.Equal(2, invalidMetadata.ExitCode);
        Assert.Contains("description", missing.Error);
        Assert.Contains("non-blank", blankDescription.Error);
        Assert.Contains("JSON object", invalidMetadata.Error);
        Assert.Equal("[]\n", list.Output);
    }
}
