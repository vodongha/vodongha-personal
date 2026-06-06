using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class ProjectService(AppDbContext db)
{
    public async Task<List<Project>> GetFeaturedAsync() =>
        await db.Projects
            .Where(p => p.IsFeatured)
            .OrderBy(p => p.Order)
            .ToListAsync();

    public async Task<List<Project>> GetAllAsync() =>
        await db.Projects.OrderBy(p => p.Order).ToListAsync();
}
