using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class ExperienceService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Experience>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Experiences.OrderBy(e => e.Order).ToListAsync();
    }
}
