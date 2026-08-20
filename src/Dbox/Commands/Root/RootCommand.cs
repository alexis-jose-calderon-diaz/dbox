using System.CommandLine;
using CommandRoot = System.CommandLine.RootCommand;
using Dbox.Cli;
using Dbox.Commands.Activity;
using Dbox.Commands.Init;

namespace Dbox.Commands.Root;

public static class RootCommand
{
    public static CommandRoot Create(CommandContext context)
    {
        var root = new CommandRoot("Local project catalog database CLI.")
        {
            TreatUnmatchedTokensAsErrors = true
        };
        root.SetAction((_, cancellationToken) => context.Executor.RunAsync(
            _ => Task.FromResult<object?>(null),
            cancellationToken,
            rootCommand: true));

        root.Add(InitCommand.Create(context));
        root.Add(ActivityCommand.Create(context));
        return root;
    }
}
