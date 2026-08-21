using System.Text.Json.Serialization;
using Dbox.Activities;

namespace Dbox.Output;

public sealed record DeleteResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("deleted")] bool Deleted);

public sealed record DeletePreviewResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("dry_run")] bool DryRun,
    [property: JsonPropertyName("activity")] ActivityView Activity);
