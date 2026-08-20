using System.CommandLine;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;
using ActivityEntity = Dbox.Activities.Activity;

namespace Dbox.Commands.Activity.Add;

public static class AddCommand
{
    public static Command Create(CommandContext context, Option<string?> outputOption)
    {
        var command = new Command("add", "Create an activity.");
        var typeOption = StringOption("--type", "Activity type.");
        var titleOption = StringOption("--title", "Activity title.");
        var descriptionOption = StringOption("--description", "Optional activity description.");
        var statusOption = StringOption("--status", "Activity status.");
        var jsonOption = StringOption("--json", "Activity input as a JSON object.");
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
        var input = ActivityInputParser.ParseCreate(
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
        var validation = ActivityValidator.ValidateCreate(input.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues);

        var activity = new ActivityEntity
        {
            Type = input.Value!.Type!,
            Title = input.Value.Title!,
            Description = input.Value.Description,
            Status = input.Value.Status ?? ActivitySchema.DefaultStatus,
            CreatedAt = DateTime.UtcNow
        };

        return await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            async (dbContext, token) => ActivityView.FromEntity(
                await context.ActivityRepository.AddAsync(dbContext, activity, token)),
            cancellationToken);
    }

    private static Option<string?> StringOption(string name, string description) =>
        new(name) { Description = description };
}
