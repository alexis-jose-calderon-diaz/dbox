using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dbox.Database;

public sealed class DboxDbContextFactory : IDesignTimeDbContextFactory<DboxDbContext>
{
    public DboxDbContext CreateDbContext(string[] args) => Create(":memory:");

    public DboxDbContext Create(string databasePath) => Create(databasePath, readOnly: false);

    public DboxDbContext CreateReadOnly(string databasePath) => Create(databasePath, readOnly: true);

    private static DboxDbContext Create(string databasePath, bool readOnly)
    {
        var connectionString = new DbConnectionStringBuilder
        {
            ["Data Source"] = databasePath
        }.ConnectionString;
        if (readOnly)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
            builder["Mode"] = "ReadOnly";
            connectionString = builder.ConnectionString;
        }

        var options = new DbContextOptionsBuilder<DboxDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new DboxDbContext(options);
    }
}
