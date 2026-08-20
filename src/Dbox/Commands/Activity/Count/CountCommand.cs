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
        var jsonOption = new Option<string?>("--json") { Description = "Filters as a JSON object." };
        command.Options.Add(jsonOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(context, parseResult, jsonOption, token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Option<string?> jsonOption,
        CancellationToken cancellationToken)
    {
        var filter = ActivityInputParser.ParseFilter(parseResult.GetValue(jsonOption));
        ActivityCommand.ThrowIfInvalid(filter.Issues);
        var validation = ActivityValidator.ValidateFilter(filter.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var count = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.CountAsync(dbContext, filter.Value!, token),
            cancellationToken);
        return new CountResponse(count);
    }
}
