using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Data.Models;

namespace VodonghaPersonal.Services;

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
