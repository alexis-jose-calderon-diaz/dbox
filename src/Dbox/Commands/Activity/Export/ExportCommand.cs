using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;

namespace Dbox.Commands.Activity.Export;

public static class ExportCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("export", "Export all activities.");
        var formatOption = new Option<string?>("--format")
        {
            Description = "Portable format: json (default) or jsonl."
        };
        command.Options.Add(formatOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(context, parseResult.GetValue(formatOption), token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        string? requestedFormat,
        CancellationToken cancellationToken)
    {
        var format = ValidateFormat(requestedFormat ?? "json");
        var activities = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.ListAllAsync(dbContext, token),
            cancellationToken);
        var views = activities.Select(ActivityView.FromEntity).ToList();
        if (format == "jsonl")
        {
            context.Writer.WriteJsonLines(views);
            return null;
        }

        return views;
    }

    private static string ValidateFormat(string format)
    {
        if (format is "json" or "jsonl")
        {
            return format;
        }

        throw CliException.Validation(
            [new ErrorDetail("format", "Value must be one of: json, jsonl.")],
            "Invalid export format.");
    }
}
