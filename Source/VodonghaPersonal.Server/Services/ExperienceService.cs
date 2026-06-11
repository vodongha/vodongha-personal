using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class ExperienceService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Experience>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Experiences.OrderBy(e => e.Order).ToListAsync();
    }
}
