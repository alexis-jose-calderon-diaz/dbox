using System.Text.Json.Serialization;

namespace Dbox.Output;

public sealed record BackupResponse(
    [property: JsonPropertyName("database")] string Database,
    [property: JsonPropertyName("backup")] string Backup);
