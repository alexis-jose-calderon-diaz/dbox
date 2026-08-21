using System.CommandLine;
using Dbox.Activities;
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
        var yesOption = new Option<bool>("--yes")
        {
            Description = "Confirm the permanent deletion."
        };
        var dryRunOption = new Option<bool>("--dry-run")
        {
            Description = "Preview the deletion without changing the database."
        };
        command.Arguments.Add(idArgument);
        command.Options.Add(yesOption);
        command.Options.Add(dryRunOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(
                context,
                parseResult.GetValue(idArgument),
                parseResult.GetValue(yesOption),
                parseResult.GetValue(dryRunOption),
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        long id,
        bool yes,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        if (!dryRun && !yes)
        {
            throw CliException.Validation(
                [new ErrorDetail("yes", "Option '--yes' is required unless '--dry-run' is provided.")],
                "Invalid command.");
        }

        if (dryRun)
        {
            var activity = await context.Database.ExecuteWithoutMigrationAsync(
                context.CurrentDirectoryProvider(),
                (dbContext, token) => context.ActivityRepository.GetAsync(dbContext, id, token),
                cancellationToken);
            return activity is null
                ? throw CliException.ResourceNotFound(id)
                : new DeletePreviewResponse(id, false, true, ActivityView.FromEntity(activity));
        }

        var deleted = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.DeleteAsync(dbContext, id, token),
            cancellationToken);
        return deleted ? new DeleteResponse(id, true) : throw CliException.ResourceNotFound(id);
    }
}
