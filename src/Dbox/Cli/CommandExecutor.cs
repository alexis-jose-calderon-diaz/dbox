using Dbox.Output;

namespace Dbox.Cli;

public sealed class CommandExecutor(OutputWriter writer)
{
    public async Task<int> RunAsync(
        Func<CancellationToken, Task<object?>> operation,
        CancellationToken cancellationToken,
        bool rootCommand = false)
    {
        try
        {
            if (rootCommand)
            {
                throw CliException.Validation([new ErrorDetail("command", "A command is required.")]);
            }

            var result = await operation(cancellationToken);
            if (result is not null)
            {
                writer.WriteSuccess(result);
            }

            return ExitCodes.Success;
        }
        catch (CliException exception)
        {
            writer.WriteError(exception.Error);
            return exception.Error.ExitCode;
        }
        catch (OperationCanceledException)
        {
            return ExitCodes.UnexpectedError;
        }
        catch (Exception)
        {
            writer.WriteError(new CliError("unexpected_error", "Unexpected error.", ExitCodes.UnexpectedError));
            return ExitCodes.UnexpectedError;
        }
    }
}
