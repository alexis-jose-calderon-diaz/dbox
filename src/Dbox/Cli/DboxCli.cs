using System.CommandLine;
using CommandRoot = System.CommandLine.RootCommand;
using Dbox.Activities;
using Dbox.Database;
using Dbox.Output;
using RootCommandBuilder = Dbox.Commands.Root.RootCommand;

namespace Dbox.Cli;

public static class DboxCli
{
    public static async Task<int> InvokeAsync(
        IReadOnlyList<string> args,
        TextWriter output,
        TextWriter error,
        string currentDirectory,
        CancellationToken cancellationToken = default,
        TextReader? input = null)
    {
        var writer = new OutputWriter(output, error);
        var root = BuildRootCommand(writer, () => currentDirectory, input ?? Console.In);
        var parseResult = root.Parse(args);

        if (parseResult.Errors.Count > 0)
        {
            var details = parseResult.Errors
                .Select(_ => new ErrorDetail("command", "Invalid command syntax."))
                .ToArray();
            writer.WriteError(
                new CliError("validation_error", "Invalid command.", ExitCodes.ValidationError, details));
            return ExitCodes.ValidationError;
        }

        var configuration = new InvocationConfiguration
        {
            Output = output,
            Error = error,
            EnableDefaultExceptionHandler = false
        };

        return await parseResult.InvokeAsync(configuration, cancellationToken);
    }

    public static CommandRoot BuildRootCommand(
        OutputWriter writer,
        Func<string> currentDirectoryProvider,
        TextReader? input = null)
    {
        var locator = new DboxLocator();
        var contextFactory = new DboxDbContextFactory();
        var database = new DboxDatabase(locator, contextFactory);
        var context = new CommandContext(
            writer,
            locator,
            database,
            new DboxDatabaseMaintenance(locator, contextFactory),
            new ActivityRepository(),
            currentDirectoryProvider,
            input ?? Console.In);
        return RootCommandBuilder.Create(context);
    }
}
