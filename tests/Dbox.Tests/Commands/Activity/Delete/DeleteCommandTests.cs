using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Delete;

public sealed class DeleteCommandTests
{
    [Fact]
    public async Task DeleteRequiresConfirmationBeforeChangingData()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Delete me\",\"description\":\"Delete details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Delete result\",\"impact\":\"Delete impact\",\"effort\":\"low\"}");

        var deleted = await TestProject.RunAsync(project.Root, "activity", "delete", "1");
        var existing = await TestProject.RunAsync(project.Root, "activity", "get", "1");

        Assert.Equal(2, deleted.ExitCode);
        Assert.Empty(deleted.Output);
        Assert.Contains("validation_error", deleted.Error);
        Assert.Equal(0, existing.ExitCode);
        Assert.Contains("Delete me", existing.Output);
    }

    [Fact]
    public async Task DeleteSupportsConfirmationAndNonMutatingPreviews()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Preview me\",\"description\":\"Preview details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Preview result\",\"impact\":\"Preview impact\",\"effort\":\"low\"}");

        var preview = await TestProject.RunAsync(project.Root, "activity", "delete", "1", "--dry-run");
        var bothFlags = await TestProject.RunAsync(project.Root, "activity", "delete", "1", "--yes", "--dry-run");
        var existing = await TestProject.RunAsync(project.Root, "activity", "get", "1");
        var deleted = await TestProject.RunAsync(project.Root, "activity", "delete", "1", "--yes");
        var missing = await TestProject.RunAsync(project.Root, "activity", "get", "1");

        Assert.Equal(0, preview.ExitCode);
        Assert.Contains("\"deleted\": false", preview.Output);
        Assert.Contains("\"dry_run\": true", preview.Output);
        Assert.Contains("\"title\": \"Preview me\"", preview.Output);
        Assert.Equal(0, bothFlags.ExitCode);
        Assert.Contains("\"dry_run\": true", bothFlags.Output);
        Assert.Equal(0, existing.ExitCode);
        Assert.Equal(0, deleted.ExitCode);
        Assert.Contains("\"deleted\": true", deleted.Output);
        Assert.Equal(3, missing.ExitCode);
        Assert.Contains("resource_not_found", missing.Error);
    }

    [Fact]
    public async Task DryRunDoesNotApplyPendingMigrations()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(databasePath, []);

        var preview = await TestProject.RunAsync(project.Root, "activity", "delete", "1", "--dry-run");

        Assert.Equal(4, preview.ExitCode);
        Assert.Empty(preview.Output);
        Assert.Contains("database_error", preview.Error);
        Assert.Empty(File.ReadAllBytes(databasePath));
    }
}
