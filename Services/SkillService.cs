using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

public class SkillService(AppDbContext db)
{
    public async Task<List<Skill>> GetAllAsync() =>
        await db.Skills.OrderBy(s => s.Order).ToListAsync();

    public async Task<Dictionary<string, List<Skill>>> GetGroupedAsync()
    {
        List<Skill> skills = await GetAllAsync();
        return skills.GroupBy(s => s.Category).ToDictionary(g => g.Key, g => g.ToList());
    }
}
