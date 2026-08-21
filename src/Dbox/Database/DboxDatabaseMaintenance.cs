using System.Globalization;
using Dbox.Cli;
using Dbox.Output;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Dbox.Database;

public sealed class DboxDatabaseMaintenance(
    DboxLocator locator,
    DboxDbContextFactory contextFactory)
{
    public async Task<BackupResponse> BackupAsync(
        string startingDirectory,
        CancellationToken cancellationToken)
    {
        var location = RequireDatabase(startingDirectory);
        var backupsDirectory = Path.Combine(location.DboxDirectory!, "backups");
        var timestamp = DateTimeOffset.UtcNow.ToString(
            "yyyyMMdd'T'HHmmssfff'Z'",
            CultureInfo.InvariantCulture);
        var backupPath = Path.Combine(backupsDirectory, $"data-{timestamp}.db");
        var reserved = false;

        try
        {
            Directory.CreateDirectory(backupsDirectory);
            ReserveDestination(backupPath);
            reserved = true;

            await using var source = CreateConnection(location.DatabasePath!, SqliteOpenMode.ReadOnly);
            await using var destination = CreateConnection(backupPath, SqliteOpenMode.ReadWrite);
            await source.OpenAsync(cancellationToken);
            await destination.OpenAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            source.BackupDatabase(destination);
            cancellationToken.ThrowIfCancellationRequested();

            return new BackupResponse(
                RelativeToProject(location, location.DatabasePath!),
                RelativeToProject(location, backupPath));
        }
        catch (OperationCanceledException)
        {
            DeleteReservedDestination(backupPath, reserved);
            throw;
        }
        catch (Exception exception)
        {
            DeleteReservedDestination(backupPath, reserved);
            throw CliException.Database(exception);
        }
    }

    public async Task<DoctorResponse> DiagnoseAsync(
        string startingDirectory,
        CancellationToken cancellationToken)
    {
        var location = RequireDatabase(startingDirectory);
        var databasePath = location.DatabasePath!;
        var permissions = InspectPermissions(
            databasePath,
            Path.Combine(location.DboxDirectory!, "backups"));
        var canOpen = false;
        var integrity = "not_checked";
        IReadOnlyList<string>? pendingMigrations = null;

        await using var connection = CreateConnection(databasePath, SqliteOpenMode.ReadOnly);
        try
        {
            await connection.OpenAsync(cancellationToken);
            canOpen = true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return new DoctorResponse(
                RelativeToProject(location, databasePath),
                Exists: true,
                CanOpen: false,
                Integrity: "not_checked",
                PendingMigrations: null,
                permissions);
        }

        integrity = await CheckIntegrityAsync(connection, cancellationToken);
        if (integrity == "ok")
        {
            pendingMigrations = await ReadPendingMigrationsAsync(databasePath, cancellationToken);
        }

        return new DoctorResponse(
            RelativeToProject(location, databasePath),
            Exists: true,
            CanOpen: canOpen,
            Integrity: integrity,
            PendingMigrations: pendingMigrations,
            permissions);
    }

    private DboxLocation RequireDatabase(string startingDirectory)
    {
        var location = locator.Find(startingDirectory);
        if (!location.DatabaseExists || location.DatabasePath is null)
        {
            throw CliException.DatabaseNotFound();
        }

        return location;
    }

    private async Task<IReadOnlyList<string>?> ReadPendingMigrationsAsync(
        string databasePath,
        CancellationToken cancellationToken)
    {
        await using var context = contextFactory.CreateReadOnly(databasePath);
        try
        {
            return (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsMissingMigrationHistory(exception))
        {
            return context.Database.GetMigrations().ToArray();
        }
        catch (Exception exception)
        {
            throw CliException.Database(exception);
        }
    }

    private static async Task<string> CheckIntegrityAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return string.Equals(result?.ToString(), "ok", StringComparison.OrdinalIgnoreCase)
                ? "ok"
                : "failed";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return "not_checked";
        }
    }

    private static SqliteConnection CreateConnection(string databasePath, SqliteOpenMode mode)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = mode
        }.ToString();
        return new SqliteConnection(connectionString);
    }

    private static void ReserveDestination(string path)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
    }

    private static void DeleteReservedDestination(string path, bool reserved)
    {
        if (!reserved)
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch
        {
            // Keep the original maintenance error when cleanup is unavailable.
        }
    }

    private static bool IsMissingMigrationHistory(Exception exception)
    {
        var message = exception.ToString();
        return message.Contains("no such table", StringComparison.OrdinalIgnoreCase)
            && message.Contains("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase);
    }

    private static DoctorPermissions InspectPermissions(string databasePath, string backupsDirectory)
    {
        return new DoctorPermissions(
            InspectPathPermission(databasePath, writable: false),
            InspectPathPermission(databasePath, writable: true),
            InspectPathPermission(backupsDirectory, writable: true));
    }

    private static bool? InspectPathPermission(string path, bool writable)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return null;
        }

        try
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                var mode = File.GetUnixFileMode(path);
                var relevantModes = writable
                    ? UnixFileMode.UserWrite | UnixFileMode.GroupWrite | UnixFileMode.OtherWrite
                    : UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead;
                return (mode & relevantModes) != (UnixFileMode)0;
            }

            var attributes = File.GetAttributes(path);
            return writable
                ? !attributes.HasFlag(FileAttributes.ReadOnly)
                : true;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }

    private static string RelativeToProject(DboxLocation location, string path)
    {
        return Path.GetRelativePath(location.ProjectDirectory!, path)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }
}
