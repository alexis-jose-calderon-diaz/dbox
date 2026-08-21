using System.Text.Json.Serialization;

namespace Dbox.Output;

public sealed record DoctorResponse(
    [property: JsonPropertyName("database")] string Database,
    [property: JsonPropertyName("exists")] bool Exists,
    [property: JsonPropertyName("can_open")] bool CanOpen,
    [property: JsonPropertyName("integrity")] string Integrity,
    [property: JsonPropertyName("pending_migrations")] IReadOnlyList<string>? PendingMigrations,
    [property: JsonPropertyName("permissions")] DoctorPermissions Permissions);

public sealed record DoctorPermissions(
    [property: JsonPropertyName("database_readable")] bool? DatabaseReadable,
    [property: JsonPropertyName("database_writable")] bool? DatabaseWritable,
    [property: JsonPropertyName("backup_directory_writable")] bool? BackupDirectoryWritable);
