using Dbox.Cli;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Root;

public sealed class RootCommandTests
{
    [Fact]
    public async Task HelpExposesRootInfrastructureAndActivityCatalog()
    {
        using var project = new TestProject();

        var rootHelp = await TestProject.RunAsync(project.Root, "--help");
        var activityHelp = await TestProject.RunAsync(project.Root, "activity", "--help");

        Assert.Equal(0, rootHelp.ExitCode);
        Assert.Empty(rootHelp.Error);
        Assert.Contains("init", rootHelp.Output);
        Assert.Contains("activity", rootHelp.Output);
        Assert.DoesNotContain("schema", rootHelp.Output);
        Assert.DoesNotContain("add", rootHelp.Output);
        Assert.Equal(0, activityHelp.ExitCode);
        Assert.Empty(activityHelp.Error);
        Assert.Contains("schema", activityHelp.Output);
        Assert.Contains("add", activityHelp.Output);
        Assert.Contains("list", activityHelp.Output);
        Assert.Contains("get", activityHelp.Output);
        Assert.Contains("update", activityHelp.Output);
        Assert.Contains("delete", activityHelp.Output);
    }

    [Fact]
    public async Task FlatActivityCommandsAndRootSchemaAliasAreRejected()
    {
        using var project = new TestProject();
        var flatCommands = new[]
        {
            new[] { "schema" },
            new[] { "add" },
            new[] { "list" },
            new[] { "get", "1" },
            new[] { "update", "1" },
            new[] { "delete", "1" }
        };

        foreach (var arguments in flatCommands)
        {
            var result = await TestProject.RunAsync(project.Root, arguments);

            Assert.Equal(ExitCodes.ValidationError, result.ExitCode);
            Assert.Empty(result.Output);
            Assert.Contains("Validation error:", result.Error);
        }

        var rootSchemaAlias = await TestProject.RunAsync(project.Root, "--schema");
        var rootSchemaAliasJson = await TestProject.RunAsync(project.Root, "--schema", "--json");

        Assert.Equal(ExitCodes.ValidationError, rootSchemaAlias.ExitCode);
        Assert.Equal(ExitCodes.ValidationError, rootSchemaAliasJson.ExitCode);
        Assert.Empty(rootSchemaAlias.Output);
        Assert.Empty(rootSchemaAliasJson.Output);
        Assert.Contains("Validation error:", rootSchemaAlias.Error);
        Assert.Contains("Validation error:", rootSchemaAliasJson.Error);
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox")));
    }
}
