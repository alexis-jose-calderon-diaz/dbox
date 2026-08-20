using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;

namespace Dbox.Commands.Activity.Update;

public static class UpdateCommand
{
    public static Command Create(CommandContext context, Option<string?> outputOption)
    {
        var command = new Command("update", "Update an activity.");
        var idArgument = new Argument<long>("id")
        {
            Description = "Activity id."
        };
        var typeOption = StringOption("--type", "New activity type.");
        var titleOption = StringOption("--title", "New activity title.");
        var descriptionOption = StringOption("--description", "New activity description.");
        var statusOption = StringOption("--status", "New activity status.");
        var jsonOption = StringOption("--json", "Activity update as a JSON object.");
        command.Arguments.Add(idArgument);
        command.Options.Add(typeOption);
        command.Options.Add(titleOption);
        command.Options.Add(descriptionOption);
        command.Options.Add(statusOption);
        command.Options.Add(jsonOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            parseResult,
            outputOption,
            forceJson: false,
            (_, token) => ExecuteAsync(
                context,
                parseResult,
                idArgument,
                typeOption,
                titleOption,
                descriptionOption,
                statusOption,
                jsonOption,
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        Argument<long> idArgument,
        Option<string?> typeOption,
        Option<string?> titleOption,
        Option<string?> descriptionOption,
        Option<string?> statusOption,
        Option<string?> jsonOption,
        CancellationToken cancellationToken)
    {
        var typeProvided = parseResult.GetResult(typeOption) is not null;
        var titleProvided = parseResult.GetResult(titleOption) is not null;
        var descriptionProvided = parseResult.GetResult(descriptionOption) is not null;
        var statusProvided = parseResult.GetResult(statusOption) is not null;
        var jsonProvided = parseResult.GetResult(jsonOption) is not null;
        var input = ActivityInputParser.ParseUpdate(
            jsonProvided,
            parseResult.GetValue(jsonOption),
            typeProvided,
            parseResult.GetValue(typeOption),
            titleProvided,
            parseResult.GetValue(titleOption),
            descriptionProvided,
            parseResult.GetValue(descriptionOption),
            statusProvided,
            parseResult.GetValue(statusOption));

        ActivityCommand.ThrowIfInvalid(input.Issues);
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

    private static Option<string?> StringOption(string name, string description) =>
        new(name) { Description = description };
}
