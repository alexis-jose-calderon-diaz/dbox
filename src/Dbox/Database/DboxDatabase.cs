using Dbox.Cli;
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
            Directory.CreateDirectory(location.DboxDirectory);
            await using var context = contextFactory.Create(location.DatabasePath);
            if (!existed)
            {
                await context.Database.MigrateAsync(cancellationToken);
                return new InitResponse(".dbox/data.db", "initialized");
            }

            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
            if (pending.Length > 0)
            {
                await context.Database.MigrateAsync(cancellationToken);
                return new InitResponse(".dbox/data.db", "migrated");
            }

            return new InitResponse(".dbox/data.db", "already_initialized");
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
        var location = locator.Find(startingDirectory);
        if (location is null || !location.DatabaseExists)
        {
            throw CliException.DatabaseNotFound();
        }

        try
        {
            await using var context = contextFactory.Create(location.DatabasePath);
            await context.Database.MigrateAsync(cancellationToken);
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
