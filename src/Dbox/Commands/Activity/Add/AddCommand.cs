using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;
using ActivityEntity = Dbox.Activities.Activity;

namespace Dbox.Commands.Activity.Add;

public static class AddCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("add", "Create an activity.");
        var jsonOption = new Option<string?>("--json") { Description = "Activity input as a JSON object." };
        command.Options.Add(jsonOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(
                context,
                parseResult,
                jsonOption,
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Option<string?> jsonOption,
        CancellationToken cancellationToken)
    {
        var input = ActivityInputParser.ParseCreate(parseResult.GetValue(jsonOption));

        ActivityCommand.ThrowIfInvalid(input.Issues);
        var validation = ActivityValidator.ValidateCreate(input.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var activity = new ActivityEntity
        {
            Type = input.Value!.Type!,
            Title = input.Value.Title!,
            Description = input.Value.Description!,
            Status = input.Value.Status!,
            Source = input.Value.Source!,
            Area = input.Value.Area!,
            Result = input.Value.Result!,
            Impact = input.Value.Impact!,
            Effort = input.Value.Effort!,
            Reference = input.Value.Reference,
            Metadata = input.Value.Metadata,
            CreatedAt = DateTime.UtcNow
        };

        return await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            async (dbContext, token) => ActivityView.FromEntity(
                await context.ActivityRepository.AddAsync(dbContext, activity, token)),
            cancellationToken);
    }
}
