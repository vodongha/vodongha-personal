using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VodonghaPersonal.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Real connection string via env (set for `dotnet ef database update`);
        // falls back to a dummy so `dotnet ef migrations add` (needs no live
        // connection) still resolves the Oracle provider.
        string cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? "User Id=VODONGHA_PERSONAL;Password=x;Data Source=x";
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseOracle(cs, o => o.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19));
        return new AppDbContext(optionsBuilder.Options);
    }
}
