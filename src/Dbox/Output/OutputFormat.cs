namespace Dbox.Output;

public enum OutputFormat
{
    Text,
    Json
}

public static class OutputFormatParser
{
    public static bool TryParse(string? value, out OutputFormat format)
    {
        if (value is null || string.Equals(value, "text", StringComparison.OrdinalIgnoreCase))
        {
            format = OutputFormat.Text;
            return true;
        }

        if (string.Equals(value, "json", StringComparison.OrdinalIgnoreCase))
        {
            format = OutputFormat.Json;
            return true;
        }

        format = default;
        return false;
    }
}
