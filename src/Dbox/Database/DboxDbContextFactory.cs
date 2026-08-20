using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dbox.Database;

public sealed class DboxDbContextFactory : IDesignTimeDbContextFactory<DboxDbContext>
{
    public DboxDbContext CreateDbContext(string[] args) => Create(":memory:");

    public DboxDbContext Create(string databasePath)
    {
        var connectionString = new DbConnectionStringBuilder
        {
            ["Data Source"] = databasePath
        }.ConnectionString;
        var options = new DbContextOptionsBuilder<DboxDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new DboxDbContext(options);
    }
}
