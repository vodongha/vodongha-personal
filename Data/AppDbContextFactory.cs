using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace vodongha.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<AppDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql("Host=localhost;Database=vodongha_dev;Username=postgres;Password=postgres");
        return new AppDbContext(optionsBuilder.Options);
    }
}
