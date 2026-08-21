using System.CommandLine;
using Dbox.Cli;

namespace Dbox.Commands.Root;

public static class BackupCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("backup", "Create a consistent backup of the project database.");
        command.SetAction((_, cancellationToken) => context.Executor.RunAsync(
            async token => (object?)await context.DatabaseMaintenance.BackupAsync(
                context.CurrentDirectoryProvider(),
                token),
            cancellationToken));
        return command;
    }
}
