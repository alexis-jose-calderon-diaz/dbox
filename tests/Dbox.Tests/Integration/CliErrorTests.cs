using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class CliErrorTests
{
    [Fact]
    public async Task TextErrorsUseTheDocumentedMessagesAndExitCodes()
    {
        using var project = new TestProject();

        var missingDatabase = await TestProject.RunAsync(project.Root, "activity", "list");
        await TestProject.RunAsync(project.Root, "init");
        var missingActivity = await TestProject.RunAsync(project.Root, "activity", "get", "7");
        var unsupportedOutput = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "yaml");
        var emptyOutput = await TestProject.RunAsync(project.Root, "activity", "list", "--output=");
        var parserJson = await TestProject.RunAsync(project.Root, "activity", "list", "--output=json", "--unknown");
        var parserJsonAfterUnknown = await TestProject.RunAsync(project.Root, "activity", "list", "--unknown", "--output=json");

        Assert.Equal(4, missingDatabase.ExitCode);
        Assert.Equal("No dbox database found.\nRun 'dbox init' to initialize this directory.\n", missingDatabase.Error);
        Assert.Equal(3, missingActivity.ExitCode);
        Assert.Equal("Activity 7 not found.\n", missingActivity.Error);
        Assert.Equal(2, unsupportedOutput.ExitCode);
        Assert.Contains("Validation error:", unsupportedOutput.Error);
        Assert.Equal(2, emptyOutput.ExitCode);
        Assert.Contains("Validation error:", emptyOutput.Error);
        Assert.Equal(2, parserJson.ExitCode);
        Assert.Empty(parserJson.Output);
        Assert.Contains("\"error\"", parserJson.Error);
        using var parserDocument = JsonDocument.Parse(parserJson.Error);
        Assert.Equal(
            "Invalid command syntax.",
            parserDocument.RootElement.GetProperty("error").GetProperty("details")[0].GetProperty("message").GetString());
        Assert.Equal(2, parserJsonAfterUnknown.ExitCode);
        Assert.Empty(parserJsonAfterUnknown.Output);
        using var parserAfterUnknownDocument = JsonDocument.Parse(parserJsonAfterUnknown.Error);
        Assert.Equal(
            "validation_error",
            parserAfterUnknownDocument.RootElement.GetProperty("error").GetProperty("code").GetString());
    }
}
