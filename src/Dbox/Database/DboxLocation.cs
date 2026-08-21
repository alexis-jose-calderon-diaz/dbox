namespace Dbox.Database;

public enum DboxDiscoveryStatus
{
    Found,
    Incomplete,
    NotFound
}

public sealed record DboxLocation(
    string CurrentDirectory,
    string? ProjectDirectory,
    string? DboxDirectory,
    string? DatabasePath,
    DboxDiscoveryStatus Status)
{
    public bool DatabaseExists => Status == DboxDiscoveryStatus.Found;
}
