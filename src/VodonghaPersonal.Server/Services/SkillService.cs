using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class SkillService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<List<Skill>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.Skills.OrderBy(s => s.Order).ToListAsync();
    }

    public async Task<Dictionary<string, List<Skill>>> GetGroupedAsync()
    {
        List<Skill> skills = await GetAllAsync();
        return skills.GroupBy(s => s.Category).ToDictionary(g => g.Key, g => g.ToList());
    }
}
