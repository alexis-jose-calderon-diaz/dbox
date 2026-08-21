using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;

namespace Dbox.Commands.Activity.Update;

public static class UpdateCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("update", "Update an activity.");
        var idArgument = new Argument<long>("id")
        {
            Description = "Activity id."
        };
        var jsonOption = new Option<string?>("--json")
        {
            Description = "Activity update as a JSON object."
        };
        var jsonFileOption = new Option<string?>("--json-file")
        {
            Description = "Read activity update from a UTF-8 JSON file, or '-' for standard input."
        };
        command.Arguments.Add(idArgument);
        command.Options.Add(jsonOption);
        command.Options.Add(jsonFileOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(
                context,
                parseResult,
                idArgument,
                jsonOption,
                jsonFileOption,
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Argument<long> idArgument,
        Option<string?> jsonOption,
        Option<string?> jsonFileOption,
        CancellationToken cancellationToken)
    {
        var source = await ActivityInputParser.ReadJsonAsync(
            parseResult.GetValue(jsonOption),
            parseResult.GetValue(jsonFileOption),
            context.Input,
            required: true,
            cancellationToken,
            baseDirectory: context.CurrentDirectoryProvider());
        ActivityCommand.ThrowIfInvalid(source.Issues, source.ErrorMessage);

        var input = ActivityInputParser.ParseUpdate(source.Value);

        ActivityCommand.ThrowIfInvalid(input.Issues, input.ErrorMessage);
        var validation = ActivityValidator.ValidateUpdate(input.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var activity = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.UpdateAsync(
                dbContext,
                parseResult.GetValue(idArgument),
                input.Value!,
                token),
            cancellationToken);
        return activity is null
            ? throw CliException.ResourceNotFound(parseResult.GetValue(idArgument))
            : ActivityView.FromEntity(activity);
    }
}
