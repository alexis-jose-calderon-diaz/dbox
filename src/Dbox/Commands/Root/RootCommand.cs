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
        var outputOption = new Option<string?>("--output")
        {
            Description = "Output format: text or json.",
            DefaultValueFactory = _ => "text",
            Recursive = true
        };

        var root = new CommandRoot("Local project catalog database CLI.")
        {
            TreatUnmatchedTokensAsErrors = true
        };
        root.Options.Add(outputOption);
        root.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            parseResult,
            outputOption,
            forceJson: false,
            (_, _) => Task.FromResult<object?>(null),
            cancellationToken,
            rootCommand: true));

        root.Add(InitCommand.Create(context, outputOption));
        root.Add(ActivityCommand.Create(context, outputOption));
        return root;
    }
}
