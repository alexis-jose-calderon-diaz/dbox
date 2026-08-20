using Dbox.Activities;
using Dbox.Database;
using Dbox.Output;

namespace Dbox.Cli;

public sealed class CommandContext
{
    public CommandContext(
        OutputWriter writer,
        DboxDatabase database,
        ActivityRepository activityRepository,
        Func<string> currentDirectoryProvider)
    {
        Writer = writer;
        Executor = new CommandExecutor(writer);
        Database = database;
        ActivityRepository = activityRepository;
        CurrentDirectoryProvider = currentDirectoryProvider;
    }

    public OutputWriter Writer { get; }

    public CommandExecutor Executor { get; }

    public DboxDatabase Database { get; }

    public ActivityRepository ActivityRepository { get; }

    public Func<string> CurrentDirectoryProvider { get; }
}
