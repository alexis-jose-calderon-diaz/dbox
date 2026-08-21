using System.CommandLine;
using Dbox.Cli;
using Dbox.Output;

namespace Dbox.Commands.Context;

public static class ContextCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("context", "Show the discovered project context.");
        command.SetAction((_, cancellationToken) => context.Executor.RunAsync(
            _ => Task.FromResult<object?>(ContextResponse.FromLocation(
                context.Locator.Find(context.CurrentDirectoryProvider()))),
            cancellationToken));
        return command;
    }
}
