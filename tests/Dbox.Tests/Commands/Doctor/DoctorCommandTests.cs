using System.Text.Json;
using Dbox.Database;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Doctor;

public sealed class DoctorCommandTests
{
    [Fact]
    public async Task DoctorReportsAHealthyDatabaseAndDoesNotCreateBackupFiles()
    {
        using var project = new TestProject();
        var init = await TestProject.RunAsync(project.Root, "init");

        var result = await TestProject.RunAsync(project.Root, "doctor");
        using var document = JsonDocument.Parse(result.Output);
        var permissions = document.RootElement.GetProperty("permissions");

        Assert.Equal(0, init.ExitCode);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(".dbox/data.db", document.RootElement.GetProperty("database").GetString());
        Assert.True(document.RootElement.GetProperty("exists").GetBoolean());
        Assert.True(document.RootElement.GetProperty("can_open").GetBoolean());
        Assert.Equal("ok", document.RootElement.GetProperty("integrity").GetString());
        Assert.Empty(document.RootElement.GetProperty("pending_migrations").EnumerateArray());
        Assert.True(permissions.TryGetProperty("database_readable", out _));
        Assert.True(permissions.TryGetProperty("database_writable", out _));
        Assert.True(permissions.TryGetProperty("backup_directory_writable", out _));
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox", "backups")));
    }

    [Fact]
    public async Task DoctorReportsPendingMigrationsWithoutChangingTheDatabase()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");
        Directory.CreateDirectory(databaseDirectory);

        var factory = new DboxDbContextFactory();
        await using (var context = factory.Create(databasePath))
        {
            await context.Database.EnsureCreatedAsync();
        }

        var before = File.ReadAllBytes(databasePath);
        var result = await TestProject.RunAsync(project.Root, "doctor");
        using var document = JsonDocument.Parse(result.Output);

        Assert.Equal(0, result.ExitCode);
        Assert.True(document.RootElement.GetProperty("can_open").GetBoolean());
        Assert.Equal("ok", document.RootElement.GetProperty("integrity").GetString());
        Assert.NotEmpty(document.RootElement.GetProperty("pending_migrations").EnumerateArray());
        Assert.Equal(before, File.ReadAllBytes(databasePath));
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox", "backups")));
    }

    [Fact]
    public async Task DoctorReportsAnUnhealthyDatabaseWithoutRepairingTheDatabase()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllText(databasePath, "not a sqlite database");
        var before = File.ReadAllBytes(databasePath);

        var result = await TestProject.RunAsync(project.Root, "doctor");
        using var document = JsonDocument.Parse(result.Output);

        Assert.Equal(0, result.ExitCode);
        Assert.True(document.RootElement.GetProperty("exists").GetBoolean());
        var canOpen = document.RootElement.GetProperty("can_open").GetBoolean();
        var integrity = document.RootElement.GetProperty("integrity").GetString();
        Assert.True(!canOpen || integrity != "ok");
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("pending_migrations").ValueKind);
        Assert.Equal(before, File.ReadAllBytes(databasePath));
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox", "backups")));
    }

    [Fact]
    public async Task DoctorReportsMissingDatabaseWithoutCreatingProjectFiles()
    {
        using var project = new TestProject();

        var result = await TestProject.RunAsync(project.Root, "doctor");
        using var error = JsonDocument.Parse(result.Error);

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Equal("database_not_found", error.RootElement.GetProperty("error").GetProperty("code").GetString());
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox")));
    }
}
