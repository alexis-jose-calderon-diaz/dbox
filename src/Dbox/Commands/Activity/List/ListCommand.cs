using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;

namespace Dbox.Commands.Activity.List;

public static class ListCommand
{
    public static Command Create(CommandContext context, Option<string?> outputOption)
    {
        var command = new Command("list", "List activities.");
        var typeOption = StringOption("--type", "Filter by activity type.");
        var statusOption = StringOption("--status", "Filter by activity status.");
        command.Options.Add(typeOption);
        command.Options.Add(statusOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            parseResult,
            outputOption,
            forceJson: false,
            (_, token) => ExecuteAsync(context, parseResult, typeOption, statusOption, token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Option<string?> typeOption,
        Option<string?> statusOption,
        CancellationToken cancellationToken)
    {
        var type = parseResult.GetValue(typeOption);
        var status = parseResult.GetValue(statusOption);
        var issues = new List<ValidationIssue>();
        if (type is not null && !ActivitySchema.IsType(type))
        {
            issues.Add(new ValidationIssue("type", $"Value must be one of: {string.Join(", ", ActivitySchema.Types)}."));
        }

        if (status is not null && !ActivitySchema.IsStatus(status))
        {
            issues.Add(new ValidationIssue("status", $"Value must be one of: {string.Join(", ", ActivitySchema.Statuses)}."));
        }

        ActivityCommand.ThrowIfInvalid(issues);
        var activities = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.ListAsync(dbContext, type, status, token),
            cancellationToken);
        return activities.Select(ActivityView.FromEntity).ToList();
    }

    private static Option<string?> StringOption(string name, string description) =>
        new(name) { Description = description };
}
