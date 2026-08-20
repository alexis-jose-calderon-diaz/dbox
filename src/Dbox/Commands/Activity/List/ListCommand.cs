using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;

namespace Dbox.Commands.Activity.List;

public static class ListCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("list", "List activities.");
        var jsonOption = new Option<string?>("--json") { Description = "Filters as a JSON object." };
        var skipOption = new Option<int?>("--skip") { Description = "Number of ordered records to skip." };
        var takeOption = new Option<int?>("--take") { Description = "Maximum number of records to return." };
        command.Options.Add(jsonOption);
        command.Options.Add(skipOption);
        command.Options.Add(takeOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(context, parseResult, jsonOption, skipOption, takeOption, token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Option<string?> jsonOption,
        Option<int?> skipOption,
        Option<int?> takeOption,
        CancellationToken cancellationToken)
    {
        var filter = ActivityInputParser.ParseFilter(parseResult.GetValue(jsonOption));
        ActivityCommand.ThrowIfInvalid(filter.Issues);
        var validation = ActivityValidator.ValidateFilter(filter.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var skip = parseResult.GetValue(skipOption) ?? 0;
        var take = parseResult.GetValue(takeOption);
        var issues = new List<ValidationIssue>();
        if (skip < 0)
        {
            issues.Add(new ValidationIssue("skip", "Value must be greater than or equal to 0."));
        }

        if (take is < 0)
        {
            issues.Add(new ValidationIssue("take", "Value must be greater than or equal to 0."));
        }

        ActivityCommand.ThrowIfInvalid(issues);
        var activities = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.ListAsync(dbContext, filter.Value!, skip, take, token),
            cancellationToken);
        return activities.Select(ActivityView.FromEntity).ToList();
    }
}
