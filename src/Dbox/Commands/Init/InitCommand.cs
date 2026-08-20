using System.CommandLine;
using Dbox.Cli;

namespace Dbox.Commands.Init;

public static class InitCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("init", "Initialize the database in the current directory.");
        command.SetAction((_, cancellationToken) => context.Executor.RunAsync(
            async token => (object?)await context.Database.InitializeAsync(
                context.CurrentDirectoryProvider(),
                token),
            cancellationToken));
        return command;
    }
}
