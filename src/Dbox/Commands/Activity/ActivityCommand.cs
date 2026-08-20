using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity.Add;
using Dbox.Commands.Activity.Delete;
using Dbox.Commands.Activity.Get;
using Dbox.Commands.Activity.List;
using Dbox.Commands.Activity.Schema;
using Dbox.Commands.Activity.Update;
using System.CommandLine;

namespace Dbox.Commands.Activity;

public static class ActivityCommand
{
    public static Command Create(CommandContext context, Option<string?> outputOption)
    {
        var command = new Command("activity", "Manage the activity catalog.");
        command.Add(SchemaCommand.Create(context, outputOption));
        command.Add(AddCommand.Create(context, outputOption));
        command.Add(ListCommand.Create(context, outputOption));
        command.Add(GetCommand.Create(context, outputOption));
        command.Add(UpdateCommand.Create(context, outputOption));
        command.Add(DeleteCommand.Create(context, outputOption));
        return command;
    }

    public static string[] NormalizeSchemaAlias(IReadOnlyList<string> args)
    {
        var activityIndex = -1;
        for (var index = 0; index < args.Count; index++)
        {
            if (string.Equals(args[index], "activity", StringComparison.Ordinal))
            {
                activityIndex = index;
                break;
            }
        }

        if (activityIndex < 0)
        {
            return args.ToArray();
        }

        var schemaIndex = -1;
        for (var index = activityIndex + 1; index < args.Count; index++)
        {
            if (string.Equals(args[index], "--schema", StringComparison.Ordinal))
            {
                schemaIndex = index;
                break;
            }
        }

        if (schemaIndex < 0)
        {
            return args.ToArray();
        }

        return [.. args.Take(schemaIndex), "schema", .. args.Skip(schemaIndex + 1)];
    }

    public static void ThrowIfInvalid(IReadOnlyList<ValidationIssue> issues)
    {
        if (issues.Count == 0)
        {
            return;
        }

        throw CliException.Validation(
            issues.Select(issue => new ErrorDetail(issue.Field, issue.Message)).ToArray());
    }
}
