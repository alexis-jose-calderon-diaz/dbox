using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class CliErrorTests
{
    [Fact]
    public async Task ErrorsAlwaysUseTheJsonEnvelopeAndExitCodes()
    {
        using var project = new TestProject();

        var missingDatabase = await TestProject.RunAsync(project.Root, "activity", "list");
        await TestProject.RunAsync(project.Root, "init");
        var missingActivity = await TestProject.RunAsync(project.Root, "activity", "get", "7");
        var unsupportedOutput = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");
        var parserJson = await TestProject.RunAsync(project.Root, "activity", "list", "--unknown");

        Assert.Equal(4, missingDatabase.ExitCode);
        Assert.Contains("database_not_found", missingDatabase.Error);
        Assert.Equal(3, missingActivity.ExitCode);
        Assert.Contains("resource_not_found", missingActivity.Error);
        Assert.Equal(2, unsupportedOutput.ExitCode);
        Assert.Contains("validation_error", unsupportedOutput.Error);
        Assert.Equal(2, parserJson.ExitCode);
        Assert.Empty(parserJson.Output);
        Assert.Contains("\"error\"", parserJson.Error);
        using var parserDocument = JsonDocument.Parse(parserJson.Error);
        Assert.Equal(
            "Invalid command syntax.",
            parserDocument.RootElement.GetProperty("error").GetProperty("details")[0].GetProperty("message").GetString());
        Assert.Equal("validation_error", parserDocument.RootElement.GetProperty("error").GetProperty("code").GetString());
    }
}
