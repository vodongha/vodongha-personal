using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class ProjectService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Project>> GetFeaturedAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Projects
            .Where(p => p.IsFeatured)
            .OrderBy(p => p.Order)
            .ToListAsync();
    }

    public async Task<List<Project>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Projects.OrderBy(p => p.Order).ToListAsync();
    }
}
