using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class EducationService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    private const string AllCacheKey = "educations_all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<List<Education>> GetAllAsync()
    {
        if (cache.TryGetValue(AllCacheKey, out List<Education>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<Education> result = await db.Educations.OrderBy(e => e.Order).ToListAsync();
        cache.Set(AllCacheKey, result, CacheTtl);
        return result;
    }

    public void InvalidateCache() => cache.Remove(AllCacheKey);
}
