using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;

namespace Dbox.Commands.Activity.Schema;

public static class SchemaCommand
{
    public static Command Create(CommandContext context, Option<string?> outputOption)
    {
        var command = new Command("schema", "Show the public activity contract.");
        var jsonOption = new Option<bool>("--json")
        {
            Description = "Render the schema as JSON."
        };
        command.Options.Add(jsonOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            parseResult,
            outputOption,
            parseResult.GetValue(jsonOption),
            (_, token) => context.Database.ExecuteAsync(
                context.CurrentDirectoryProvider(),
                (_, _) => Task.FromResult<object?>(ActivitySchema.CreateDocument()),
                token),
            cancellationToken));
        return command;
    }
}
