using System.CommandLine;
using Dbox.Cli;

namespace Dbox.Commands.Init;

public static class InitCommand
{
    public static Command Create(CommandContext context, Option<string?> outputOption)
    {
        var command = new Command("init", "Initialize the database in the current directory.");
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            parseResult,
            outputOption,
            forceJson: false,
            async (_, token) => (object?)await context.Database.InitializeAsync(
                context.CurrentDirectoryProvider(),
                token),
            cancellationToken));
        return command;
    }
}
