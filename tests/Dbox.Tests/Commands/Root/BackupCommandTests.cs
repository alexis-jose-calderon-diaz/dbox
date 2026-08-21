using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Root;

public sealed class BackupCommandTests
{
    [Fact]
    public async Task BackupUsesTheNearestProjectAndProducesAReadableUtcSnapshot()
    {
        using var project = new TestProject();
        using var copiedProject = new TestProject();
        var nested = project.CreateChild("src/feature");

        var init = await TestProject.RunAsync(project.Root, "init");
        var add = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Backup me\",\"description\":\"Keep this activity\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Copied\",\"impact\":\"Preserved\",\"effort\":\"low\"}");

        var result = await TestProject.RunAsync(nested, "backup");
        using var document = JsonDocument.Parse(result.Output);
        var backupRelative = document.RootElement.GetProperty("backup").GetString()!;
        var backupPath = Path.Combine(project.Root, ".dbox", "backups", Path.GetFileName(backupRelative));

        Assert.Equal(0, init.ExitCode);
        Assert.Equal(0, add.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(".dbox/data.db", document.RootElement.GetProperty("database").GetString());
        Assert.Matches(
            @"^\.dbox/backups/data-\d{8}T\d{9}Z\.db$",
            backupRelative);
        Assert.True(File.Exists(backupPath));

        var copiedDatabaseDirectory = Path.Combine(copiedProject.Root, ".dbox");
        Directory.CreateDirectory(copiedDatabaseDirectory);
        File.Copy(backupPath, Path.Combine(copiedDatabaseDirectory, "data.db"));
        var copiedList = await TestProject.RunAsync(copiedProject.Root, "activity", "list");

        Assert.Equal(0, copiedList.ExitCode);
        Assert.Contains("Backup me", copiedList.Output);
    }

    [Fact]
    public async Task BackupReportsMissingDatabaseWithoutCreatingProjectFiles()
    {
        using var project = new TestProject();

        var result = await TestProject.RunAsync(project.Root, "backup");
        using var error = JsonDocument.Parse(result.Error);

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Equal("database_not_found", error.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox")));
    }

    [Fact]
    public async Task BackupMapsDestinationFailuresToDatabaseErrors()
    {
        using var project = new TestProject();
        var init = await TestProject.RunAsync(project.Root, "init");
        var backupsPath = Path.Combine(project.Root, ".dbox", "backups");
        File.WriteAllText(backupsPath, "not a directory");

        var result = await TestProject.RunAsync(project.Root, "backup");

        Assert.Equal(0, init.ExitCode);
        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("database_error", result.Error);
    }
}
