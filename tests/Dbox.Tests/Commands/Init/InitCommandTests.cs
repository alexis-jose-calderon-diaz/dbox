using Dbox.Database;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Init;

public sealed class InitCommandTests
{
    [Fact]
    public async Task InitIsIdempotentAndPreservesActivities()
    {
        using var project = new TestProject();

        var firstInit = await TestProject.RunAsync(project.Root, "init");
        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"implementation\",\"title\":\"Keep me\",\"description\":\"Preserve this activity\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Activity preserved\",\"impact\":\"Keeps data stable\",\"effort\":\"low\"}");
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
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Parent\",\"description\":\"Parent details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Parent result\",\"impact\":\"Parent impact\",\"effort\":\"low\"}");
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

    [Fact]
    public async Task InitUsesPrivateModesOnLinuxAndPreservesExistingArtifacts()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        using var project = new TestProject();
        var firstInit = await TestProject.RunAsync(project.Root, "init");
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");

        await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Preserve me\",\"description\":\"Details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"low\"}");
        var broadMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;
        File.SetUnixFileMode(databaseDirectory, broadMode);
        File.SetUnixFileMode(databasePath, broadMode);

        var secondInit = await TestProject.RunAsync(project.Root, "init");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");
        var privateDirectoryMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        var privateFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;

        Assert.Equal(0, firstInit.ExitCode);
        Assert.Equal(0, secondInit.ExitCode);
        Assert.Equal(privateDirectoryMode, File.GetUnixFileMode(databaseDirectory));
        Assert.Equal(privateFileMode, File.GetUnixFileMode(databasePath));

        Assert.Contains("Preserve me", list.Output);
    }

    [Fact]
    public void LinuxPermissionHardeningNormalizesPresentSqliteSidecars()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(databasePath, [1, 2, 3]);
        var sidecars = new[] { "-wal", "-shm", "-journal" }
            .Select(suffix => databasePath + suffix)
            .ToArray();
        foreach (var sidecar in sidecars)
        {
            File.WriteAllBytes(sidecar, [4, 5, 6]);
        }

        var broadMode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;
        File.SetUnixFileMode(databasePath, broadMode);
        foreach (var sidecar in sidecars)
        {
            File.SetUnixFileMode(sidecar, broadMode);
        }

        DboxFilePermissions.HardenDatabase(databasePath);

        var privateFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        Assert.Equal(privateFileMode, File.GetUnixFileMode(databasePath));
        foreach (var sidecar in sidecars)
        {
            Assert.Equal(privateFileMode, File.GetUnixFileMode(sidecar));
            Assert.Equal(new byte[] { 4, 5, 6 }, File.ReadAllBytes(sidecar));
        }
    }
}
