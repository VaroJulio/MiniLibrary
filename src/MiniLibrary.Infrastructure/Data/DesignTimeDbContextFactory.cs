using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MiniLibrary.Infrastructure.Data;

/// <summary>
/// Design-time factory for generating EF Core migrations without requiring
/// the API startup project to execute.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=MiniLibraryDb;User Id=sa;Password=placeholder;TrustServerCertificate=true;",
            sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
