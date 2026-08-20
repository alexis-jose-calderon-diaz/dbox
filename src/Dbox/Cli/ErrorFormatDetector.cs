using Dbox.Output;

namespace Dbox.Cli;

public static class ErrorFormatDetector
{
    public static OutputFormat Detect(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count; index++)
        {
            if (args[index] == "--output" &&
                index + 1 < args.Count &&
                string.Equals(args[index + 1], "json", StringComparison.OrdinalIgnoreCase))
            {
                return OutputFormat.Json;
            }

            if (args[index].StartsWith("--output=", StringComparison.Ordinal) &&
                string.Equals(args[index]["--output=".Length..], "json", StringComparison.OrdinalIgnoreCase))
            {
                return OutputFormat.Json;
            }
        }

        var activityIndex = Array.IndexOf(args.ToArray(), "activity");
        var schemaIndex = activityIndex + 1;
        if (activityIndex >= 0 &&
            schemaIndex < args.Count &&
            args[schemaIndex] == "schema" &&
            args.Skip(schemaIndex + 1).Contains("--json", StringComparer.Ordinal))
        {
            return OutputFormat.Json;
        }

        return OutputFormat.Text;
    }
}
