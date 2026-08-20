using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;

namespace Dbox.Commands.Activity.Schema;

public static class SchemaCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("schema", "Show the public activity contract.");
        command.SetAction((_, cancellationToken) => context.Executor.RunAsync(
            token => context.Database.ExecuteAsync(
                context.CurrentDirectoryProvider(),
                (_, _) => Task.FromResult<object?>(ActivitySchema.CreateDocument()),
                token),
            cancellationToken));
        return command;
    }
}
