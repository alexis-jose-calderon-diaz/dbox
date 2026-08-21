using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;
using Dbox.Output;

namespace Dbox.Commands.Activity.Count;

public static class CountCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("count", "Count activities.");
        var jsonOption = new Option<string?>("--json")
        {
            Description = "Filters as a JSON object: type, status, area, source, effort, created_from, created_to, title, description."
        };
        var jsonFileOption = new Option<string?>("--json-file")
        {
            Description = "Read filters from a UTF-8 JSON file, or '-' for standard input."
        };
        command.Options.Add(jsonOption);
        command.Options.Add(jsonFileOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(context, parseResult, jsonOption, jsonFileOption, token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Option<string?> jsonOption,
        Option<string?> jsonFileOption,
        CancellationToken cancellationToken)
    {
        var source = await ActivityInputParser.ReadJsonAsync(
            parseResult.GetValue(jsonOption),
            parseResult.GetValue(jsonFileOption),
            context.Input,
            required: false,
            cancellationToken);
        ActivityCommand.ThrowIfInvalid(source.Issues, source.ErrorMessage);

        var filter = ActivityInputParser.ParseFilter(source.Value);
        ActivityCommand.ThrowIfInvalid(filter.Issues, filter.ErrorMessage);
        var validation = ActivityValidator.ValidateFilter(filter.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var count = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.CountAsync(dbContext, filter.Value!, token),
            cancellationToken);
        return new CountResponse(count);
    }
}
