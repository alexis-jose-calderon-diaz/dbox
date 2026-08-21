using System.Text.Json.Serialization;
using Dbox.Database;

namespace Dbox.Output;

public sealed record ContextResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("cwd")] string Cwd,
    [property: JsonPropertyName("project_directory")] string? ProjectDirectory,
    [property: JsonPropertyName("dbox_directory")] string? DboxDirectory,
    [property: JsonPropertyName("database")] string? Database)
{
    public static ContextResponse FromLocation(DboxLocation location) =>
        new(
            location.Status switch
            {
                DboxDiscoveryStatus.Found => "found",
                DboxDiscoveryStatus.Incomplete => "incomplete",
                _ => "not_found"
            },
            location.CurrentDirectory,
            location.ProjectDirectory,
            location.DboxDirectory,
            location.DatabasePath);
}
