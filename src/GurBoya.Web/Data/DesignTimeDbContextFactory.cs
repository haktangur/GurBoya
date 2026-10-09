using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GurBoya.Web.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Migration generation needs a provider, not live credentials or a DB connection.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=gurboya;Username=gurboya_migrator",
                postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "infrastructure"))
            .Options;
        return new AppDbContext(options);
    }
}
