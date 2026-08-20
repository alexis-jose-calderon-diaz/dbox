using System.CommandLine;
using Dbox.Cli;
using Dbox.Output;

namespace Dbox.Commands.Activity.Delete;

public static class DeleteCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("delete", "Delete an activity.");
        var idArgument = new Argument<long>("id")
        {
            Description = "Activity id."
        };
        command.Arguments.Add(idArgument);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(
                context,
                parseResult.GetValue(idArgument),
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        long id,
        CancellationToken cancellationToken)
    {
        var deleted = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.DeleteAsync(dbContext, id, token),
            cancellationToken);
        return deleted ? new DeleteResponse(id, true) : throw CliException.ResourceNotFound(id);
    }
}
