using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class ExperienceService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    private const string AllCacheKey = "experiences_all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<List<Experience>> GetAllAsync()
    {
        if (cache.TryGetValue(AllCacheKey, out List<Experience>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<Experience> result = await db.Experiences.OrderBy(e => e.Order).ToListAsync();
        cache.Set(AllCacheKey, result, CacheTtl);
        return result;
    }

    public void InvalidateCache() => cache.Remove(AllCacheKey);
}
