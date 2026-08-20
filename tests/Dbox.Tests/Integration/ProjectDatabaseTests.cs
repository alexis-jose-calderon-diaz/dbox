using Dbox.Tests.Support;

namespace Dbox.Tests.Integration;

public sealed class ProjectDatabaseTests
{
    [Fact]
    public async Task ParentDatabaseIsDiscoveredFromNestedDirectory()
    {
        using var project = new TestProject();
        var nested = project.CreateChild("src/feature");

        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", "Parent activity");
        var result = await TestProject.RunAsync(nested, "activity", "list", "--output", "json");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Parent activity", result.Output);
    }

    [Fact]
    public async Task IncompleteNearestDatabaseBlocksParentDiscovery()
    {
        using var project = new TestProject();
        var child = project.CreateChild();
        Directory.CreateDirectory(Path.Combine(child, ".dbox"));

        await TestProject.RunAsync(project.Root, "init");
        var result = await TestProject.RunAsync(child, "activity", "list");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("No dbox database found.", result.Error);
        Assert.Contains("Run 'dbox init' to initialize this directory.", result.Error);
    }

    [Fact]
    public async Task DatabasePathsWithConnectionStringCharactersRemainUsable()
    {
        using var project = new TestProject();
        var specialDirectory = project.CreateChild("project;Mode=Memory'quoted");

        var init = await TestProject.RunAsync(specialDirectory, "init");
        var list = await TestProject.RunAsync(specialDirectory, "activity", "list", "--output", "json");

        Assert.Equal(0, init.ExitCode);
        Assert.Equal(0, list.ExitCode);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task ExistingEmptyDatabaseIsMigratedBeforeACommand()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(Path.Combine(databaseDirectory, "data.db"), []);

        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(0, list.ExitCode);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task InvalidDatabaseReturnsDatabaseErrorWithoutOutput()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllText(Path.Combine(databaseDirectory, "data.db"), "not a sqlite database");

        var result = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");
        var init = await TestProject.RunAsync(project.Root, "init", "--output", "json");

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("database_error", result.Error);
        Assert.Equal(4, init.ExitCode);
        Assert.Empty(init.Output);
        Assert.Contains("database_error", init.Error);
    }
}
