using System.CommandLine;
using Dbox.Output;

namespace Dbox.Cli;

public sealed class CommandExecutor(OutputWriter writer)
{
    public async Task<int> RunAsync(
        ParseResult parseResult,
        Option<string?> outputOption,
        bool forceJson,
        Func<OutputFormat, CancellationToken, Task<object?>> operation,
        CancellationToken cancellationToken,
        bool rootCommand = false)
    {
        var format = forceJson ? OutputFormat.Json : OutputFormat.Text;
        try
        {
            var configuredOutput = parseResult.GetValue(outputOption);
            if (!OutputFormatParser.TryParse(configuredOutput, out var parsedFormat))
            {
                throw CliException.Validation([new ErrorDetail("output", "Value must be text or json.")]);
            }

            if (!forceJson)
            {
                format = parsedFormat;
            }

            if (rootCommand)
            {
                throw CliException.Validation([new ErrorDetail("command", "A command is required.")]);
            }

            var result = await operation(format, cancellationToken);
            if (result is not null)
            {
                writer.WriteSuccess(result, format);
            }

            return ExitCodes.Success;
        }
        catch (CliException exception)
        {
            writer.WriteError(exception.Error, format);
            return exception.Error.ExitCode;
        }
        catch (OperationCanceledException)
        {
            return ExitCodes.UnexpectedError;
        }
        catch (Exception)
        {
            writer.WriteError(new CliError("unexpected_error", "Unexpected error.", ExitCodes.UnexpectedError), format);
            return ExitCodes.UnexpectedError;
        }
    }
}
