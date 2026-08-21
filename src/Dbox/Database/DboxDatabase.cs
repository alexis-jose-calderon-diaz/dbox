using Dbox.Cli;
using Dbox.Output;
using Microsoft.EntityFrameworkCore;

namespace Dbox.Database;

public sealed class DboxDatabase(DboxLocator locator, DboxDbContextFactory contextFactory)
{
    public async Task<InitResponse> InitializeAsync(string currentDirectory, CancellationToken cancellationToken)
    {
        var location = locator.ForInit(currentDirectory);
        var existed = location.DatabaseExists;

        try
        {
            Directory.CreateDirectory(location.DboxDirectory!);
            DboxFilePermissions.HardenDirectory(location.DboxDirectory!);

            string status;
            await using (var context = contextFactory.Create(location.DatabasePath!))
            {
                if (!existed)
                {
                    await context.Database.MigrateAsync(cancellationToken);
                    status = "initialized";
                }
                else
                {
                    var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
                    if (pending.Length > 0)
                    {
                        await context.Database.MigrateAsync(cancellationToken);
                        status = "migrated";
                    }
                    else
                    {
                        status = "already_initialized";
                    }
                }
            }

            DboxFilePermissions.HardenDatabase(location.DatabasePath!);
            return new InitResponse(".dbox/data.db", status);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CliException.Database(exception);
        }
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        string startingDirectory,
        Func<DboxDbContext, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        return await ExecuteCoreAsync(startingDirectory, operation, migrate: true, cancellationToken);
    }

    public async Task<TResult> ExecuteWithoutMigrationAsync<TResult>(
        string startingDirectory,
        Func<DboxDbContext, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        return await ExecuteCoreAsync(startingDirectory, operation, migrate: false, cancellationToken);
    }

    private async Task<TResult> ExecuteCoreAsync<TResult>(
        string startingDirectory,
        Func<DboxDbContext, CancellationToken, Task<TResult>> operation,
        bool migrate,
        CancellationToken cancellationToken)
    {
        var location = locator.Find(startingDirectory);
        if (!location.DatabaseExists || location.DatabasePath is null)
        {
            throw CliException.DatabaseNotFound();
        }

        try
        {
            await using var context = contextFactory.Create(location.DatabasePath);
            if (migrate)
            {
                await context.Database.MigrateAsync(cancellationToken);
            }

            return await operation(context, cancellationToken);
        }
        catch (CliException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CliException.Database(exception);
        }
    }

}
