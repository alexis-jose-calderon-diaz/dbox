namespace Dbox.Database;

public sealed record DboxLocation(
    string ProjectDirectory,
    string DboxDirectory,
    string DatabasePath,
    bool DatabaseExists);
