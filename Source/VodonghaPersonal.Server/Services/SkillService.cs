using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class SkillService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    private const string AllCacheKey = "skills_all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<List<Skill>> GetAllAsync()
    {
        if (cache.TryGetValue(AllCacheKey, out List<Skill>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<Skill> result = await db.Skills.OrderBy(s => s.Order).ToListAsync();
        cache.Set(AllCacheKey, result, CacheTtl);
        return result;
    }

    public async Task<Dictionary<string, List<Skill>>> GetGroupedAsync()
    {
        List<Skill> skills = await GetAllAsync();
        return skills.GroupBy(s => s.Category).ToDictionary(g => g.Key, g => g.ToList());
    }

    public void InvalidateCache() => cache.Remove(AllCacheKey);
}
