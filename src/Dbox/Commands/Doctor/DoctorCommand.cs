using System.CommandLine;
using Dbox.Cli;

namespace Dbox.Commands.Doctor;

public static class DoctorCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("doctor", "Diagnose the project database without modifying it.");
        command.SetAction((_, cancellationToken) => context.Executor.RunAsync(
            async token => (object?)await context.DatabaseMaintenance.DiagnoseAsync(
                context.CurrentDirectoryProvider(),
                token),
            cancellationToken));
        return command;
    }
}
