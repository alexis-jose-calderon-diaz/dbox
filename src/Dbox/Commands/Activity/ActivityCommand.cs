using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity.Add;
using Dbox.Commands.Activity.Count;
using Dbox.Commands.Activity.Delete;
using Dbox.Commands.Activity.Get;
using Dbox.Commands.Activity.List;
using Dbox.Commands.Activity.Schema;
using Dbox.Commands.Activity.Update;
using System.CommandLine;

namespace Dbox.Commands.Activity;

public static class ActivityCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("activity", "Manage the activity catalog.");
        command.Add(SchemaCommand.Create(context));
        command.Add(AddCommand.Create(context));
        command.Add(ListCommand.Create(context));
        command.Add(CountCommand.Create(context));
        command.Add(GetCommand.Create(context));
        command.Add(UpdateCommand.Create(context));
        command.Add(DeleteCommand.Create(context));
        return command;
    }

    public static void ThrowIfInvalid(IReadOnlyList<ValidationIssue> issues, string? message = null)
    {
        if (issues.Count == 0)
        {
            return;
        }

        throw CliException.Validation(
            issues.Select(issue => new ErrorDetail(issue.Field, issue.Message)).ToArray(),
            message ?? "Invalid activity.");
    }
}
