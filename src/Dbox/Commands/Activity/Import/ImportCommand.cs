using System.CommandLine;
using System.Text;
using Dbox.Activities;
using Dbox.Cli;
using Dbox.Commands.Activity;
using Dbox.Output;

namespace Dbox.Commands.Activity.Import;

public static class ImportCommand
{
    public static Command Create(CommandContext context)
    {
        var command = new Command("import", "Import complete activities.");
        var fileOption = new Option<string?>("--file")
        {
            Description = "Path to a UTF-8 JSON or JSONL export file."
        };
        var formatOption = new Option<string?>("--format")
        {
            Description = "Portable format: json or jsonl."
        };
        command.Options.Add(fileOption);
        command.Options.Add(formatOption);
        command.SetAction((parseResult, cancellationToken) => context.Executor.RunAsync(
            token => ExecuteAsync(
                context,
                parseResult.GetValue(fileOption),
                parseResult.GetValue(formatOption),
                token),
            cancellationToken));
        return command;
    }

    private static async Task<object?> ExecuteAsync(
        CommandContext context,
        string? filePath,
        string? requestedFormat,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw CliException.Validation(
                [new ErrorDetail("file", "Option '--file' is required.")],
                "Invalid command.");
        }

        if (string.IsNullOrWhiteSpace(requestedFormat) ||
            requestedFormat is not ("json" or "jsonl"))
        {
            throw CliException.Validation(
                [new ErrorDetail("format", "Option '--format' must be json or jsonl.")],
                "Invalid command.");
        }

        var path = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(context.CurrentDirectoryProvider(), filePath);
        var content = await ReadFileAsync(path, cancellationToken);
        var parsed = PortableActivityParser.Parse(content, requestedFormat);
        ActivityCommand.ThrowIfInvalid(parsed.Issues, "Invalid activity import.");

        var validation = ActivityValidator.ValidateImport(parsed.Value!);
        ActivityCommand.ThrowIfInvalid(validation.Issues, "Invalid activity import.");

        var imported = await context.Database.ExecuteAsync(
            context.CurrentDirectoryProvider(),
            (dbContext, token) => context.ActivityRepository.ImportAsync(
                dbContext,
                parsed.Value!,
                token),
            cancellationToken);
        return new ActivityImportResponse(imported, requestedFormat);
    }

    private static async Task<string> ReadFileAsync(
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            return await File.ReadAllTextAsync(
                path,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw CliException.Io(exception);
        }
    }
}
