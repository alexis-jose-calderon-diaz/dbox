using System.Text.Json.Serialization;
using Dbox.Activities;

namespace Dbox.Output;

public sealed record ActivityListResponse(
    [property: JsonPropertyName("items")] IReadOnlyList<ActivityView> Items,
    [property: JsonPropertyName("pagination")] ActivityPagination Pagination);

public sealed record ActivityPagination(
    [property: JsonPropertyName("skip")] int Skip,
    [property: JsonPropertyName("take")] int? Take,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("has_more")] bool HasMore);
