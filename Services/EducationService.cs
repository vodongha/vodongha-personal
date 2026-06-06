using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class EducationService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Education>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Educations.OrderBy(e => e.Order).ToListAsync();
    }
}
