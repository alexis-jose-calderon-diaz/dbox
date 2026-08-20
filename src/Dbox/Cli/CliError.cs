using System.Text.Json.Serialization;

namespace Dbox.Cli;

public sealed record CliError(
    string Code,
    string Message,
    int ExitCode,
    IReadOnlyList<ErrorDetail>? Details = null);

public sealed record ErrorDetail(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")] string Message);

public sealed class CliException(CliError error) : Exception(error.Message)
{
    public CliError Error { get; } = error;

    public static CliException Validation(IReadOnlyList<ErrorDetail> details, string message = "Invalid activity.") =>
        new(new CliError("validation_error", message, ExitCodes.ValidationError, details));

    public static CliException DatabaseNotFound() =>
        new(new CliError(
            "database_not_found",
            "No dbox database found.\nRun 'dbox init' to initialize this directory.",
            ExitCodes.DatabaseError));

    public static CliException Database(Exception _) =>
        new(new CliError("database_error", "Database error.", ExitCodes.DatabaseError));

    public static CliException ResourceNotFound(long id) =>
        new(new CliError("resource_not_found", $"Activity {id} not found.", ExitCodes.ResourceNotFound));

    public static CliException Unexpected() =>
        new(new CliError("unexpected_error", "Unexpected error.", ExitCodes.UnexpectedError));
}
