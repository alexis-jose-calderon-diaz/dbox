using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;

namespace Dbox.Commands.Activity.Get;

public static class GetCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("get", "Get one activity.");
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
        var activity = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.GetAsync(dbContext, id, token),
            cancellationToken);
        return activity is null
            ? throw CliException.ResourceNotFound(id)
            : ActivityView.FromEntity(activity);
    }
}
