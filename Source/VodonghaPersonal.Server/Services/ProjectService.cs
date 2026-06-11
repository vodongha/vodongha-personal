using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class ProjectService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    private const string AllCacheKey = "projects_all";
    private const string FeaturedCacheKey = "projects_featured";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<List<Project>> GetFeaturedAsync()
    {
        if (cache.TryGetValue(FeaturedCacheKey, out List<Project>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<Project> result = await db.Projects
            .Where(p => p.IsFeatured)
            .OrderBy(p => p.Order)
            .ToListAsync();
        cache.Set(FeaturedCacheKey, result, CacheTtl);
        return result;
    }

    public async Task<List<Project>> GetAllAsync()
    {
        if (cache.TryGetValue(AllCacheKey, out List<Project>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<Project> result = await db.Projects.OrderBy(p => p.Order).ToListAsync();
        cache.Set(AllCacheKey, result, CacheTtl);
        return result;
    }

    public void InvalidateCache()
    {
        cache.Remove(AllCacheKey);
        cache.Remove(FeaturedCacheKey);
    }
}
