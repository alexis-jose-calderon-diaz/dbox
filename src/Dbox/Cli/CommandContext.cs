using Dbox.Activities;
using Dbox.Database;
using Dbox.Output;

namespace Dbox.Cli;

public sealed class CommandContext
{
    public CommandContext(
        OutputWriter writer,
        DboxLocator locator,
        DboxDatabase database,
        DboxDatabaseMaintenance databaseMaintenance,
        ActivityRepository activityRepository,
        Func<string> currentDirectoryProvider)
    {
        Writer = writer;
        Executor = new CommandExecutor(writer);
        Locator = locator;
        Database = database;
        DatabaseMaintenance = databaseMaintenance;
        ActivityRepository = activityRepository;
        CurrentDirectoryProvider = currentDirectoryProvider;
    }

    public OutputWriter Writer { get; }

    public CommandExecutor Executor { get; }

    public DboxLocator Locator { get; }

    public DboxDatabase Database { get; }

    public DboxDatabaseMaintenance DatabaseMaintenance { get; }

    public ActivityRepository ActivityRepository { get; }

    public Func<string> CurrentDirectoryProvider { get; }
}
