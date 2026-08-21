using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;
using Dbox.Output;

namespace Dbox.Commands.Activity.List;

public static class ListCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("list", "List activities.");
        var jsonOption = new Option<string?>("--json")
        {
            Description = "Filters as a JSON object: type, status, area, source, effort, created_from, created_to, title, description."
        };
        var jsonFileOption = new Option<string?>("--json-file")
        {
            Description = "Read filters from a UTF-8 JSON file, or '-' for standard input."
        };
        var skipOption = new Option<int?>("--skip")
        {
            Description = "Number of ordered records to skip. Defaults to 0."
        };
        var takeOption = new Option<int?>("--take")
        {
            Description = "Maximum number of records to return. Defaults to 100."
        };
        var allOption = new Option<bool>("--all")
        {
            Description = "Return all matching records without a limit."
        };
        command.Options.Add(jsonOption);
        command.Options.Add(jsonFileOption);
        command.Options.Add(skipOption);
        command.Options.Add(takeOption);
        command.Options.Add(allOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(
                context,
                parseResult,
                jsonOption,
                jsonFileOption,
                skipOption,
                takeOption,
                allOption,
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Option<string?> jsonOption,
        Option<string?> jsonFileOption,
        Option<int?> skipOption,
        Option<int?> takeOption,
        Option<bool> allOption,
        CancellationToken cancellationToken)
    {
        var source = await ActivityInputParser.ReadJsonAsync(
            parseResult.GetValue(jsonOption),
            parseResult.GetValue(jsonFileOption),
            context.Input,
            required: false,
            cancellationToken,
            baseDirectory: context.CurrentDirectoryProvider());
        ActivityCommand.ThrowIfInvalid(source.Issues, source.ErrorMessage);

        var filter = ActivityInputParser.ParseFilter(source.Value);
        ActivityCommand.ThrowIfInvalid(filter.Issues, filter.ErrorMessage);
        var validation = ActivityValidator.ValidateFilter(filter.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var skip = parseResult.GetValue(skipOption) ?? 0;
        var specifiedTake = parseResult.GetValue(takeOption);
        var all = parseResult.GetValue(allOption);
        var issues = new List<ValidationIssue>();
        if (skip < 0)
        {
            issues.Add(new ValidationIssue("skip", "Value must be greater than or equal to 0."));
        }

        if (specifiedTake is < 0)
        {
            issues.Add(new ValidationIssue("take", "Value must be greater than or equal to 0."));
        }

        if (all && specifiedTake is not null)
        {
            issues.Add(new ValidationIssue(
                "take",
                "Options '--all' and '--take' cannot be used together."));
        }

        ActivityCommand.ThrowIfInvalid(issues, all && specifiedTake is not null
            ? "Options '--all' and '--take' cannot be used together."
            : null);

        int? take = all ? null : specifiedTake ?? 100;
        var page = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.ListPageAsync(
                dbContext,
                filter.Value!,
                skip,
                take,
                token),
            cancellationToken);
        var items = page.Items.Select(ActivityView.FromEntity).ToList();
        var hasMore = take is not null && (long)skip + take.Value < page.Total;
        return new ActivityListResponse(
            items,
            new ActivityPagination(skip, take, page.Total, hasMore));
    }
}
