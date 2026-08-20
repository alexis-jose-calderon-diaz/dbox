using System.Text.Json;
using System.Text.Json.Serialization;
using Dbox.Cli;

namespace Dbox.Output;

public sealed class OutputWriter(TextWriter output, TextWriter error)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public void WriteSuccess(object? value)
    {
        output.WriteLine(JsonSerializer.Serialize(value, JsonOptions));
    }

    public void WriteError(CliError cliError)
    {
        var envelope = new ErrorEnvelope
        {
            Error = new ErrorBody
            {
                Code = cliError.Code,
                Message = cliError.Message,
                Details = cliError.Details
            }
        };
        error.WriteLine(JsonSerializer.Serialize(envelope, JsonOptions));
    }

    private sealed class ErrorEnvelope
    {
        [JsonPropertyName("error")]
        public ErrorBody Error { get; init; } = new();
    }

    private sealed class ErrorBody
    {
        [JsonPropertyName("code")]
        public string Code { get; init; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<ErrorDetail>? Details { get; init; }
    }
}
