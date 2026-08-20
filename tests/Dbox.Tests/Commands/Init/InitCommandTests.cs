using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Init;

public sealed class InitCommandTests
{
    [Fact]
    public async Task InitIsIdempotentAndPreservesActivities()
    {
        using var project = new TestProject();

        var firstInit = await TestProject.RunAsync(project.Root, "init");
        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"implementation\",\"title\":\"Keep me\"}");
        var secondInit = await TestProject.RunAsync(project.Root, "init");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(0, firstInit.ExitCode);
        Assert.Contains("\"status\": \"initialized\"", firstInit.Output);
        Assert.Equal(0, add.ExitCode);
        Assert.Equal(0, secondInit.ExitCode);
        Assert.Contains("\"status\": \"already_initialized\"", secondInit.Output);
        Assert.Equal(0, list.ExitCode);
        Assert.Contains("Keep me", list.Output);
        Assert.True(File.Exists(Path.Combine(project.Root, ".dbox", "data.db")));
    }

    [Fact]
    public async Task NestedInitializationUsesAnIndependentDatabase()
    {
        using var project = new TestProject();
        var child = project.CreateChild();

        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Parent\"}");
        var childInit = await TestProject.RunAsync(child, "init");
        var childList = await TestProject.RunAsync(child, "activity", "list");
        var parentList = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(0, childInit.ExitCode);
        Assert.Equal("[]\n", childList.Output);
        Assert.Contains("Parent", parentList.Output);
    }

    [Fact]
    public async Task InitReportsMigrationForAnExistingDatabaseWithPendingMigrations()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(Path.Combine(databaseDirectory, "data.db"), []);

        var init = await TestProject.RunAsync(project.Root, "init");

        Assert.Equal(0, init.ExitCode);
        Assert.Contains("\"status\": \"migrated\"", init.Output);
    }
}
