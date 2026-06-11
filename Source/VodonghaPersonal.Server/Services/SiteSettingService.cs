using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class SiteSettingService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    private const string AllCacheKey = "sitesettings_all";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        if (cache.TryGetValue(AllCacheKey, out Dictionary<string, string>? cached) && cached is not null)
        {
            return cached;
        }
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        Dictionary<string, string> result = await db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        cache.Set(AllCacheKey, result, CacheTtl);
        return result;
    }

    public string Get(Dictionary<string, string> settings, string key, string fallback = "")
        => settings.TryGetValue(key, out string? val) ? val : fallback;

    public async Task<string?> GetAsync(string key)
    {
        Dictionary<string, string> all = await GetAllAsync();
        return all.TryGetValue(key, out string? val) ? val : null;
    }

    public async Task SetAsync(string key, string value)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        SiteSetting? setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            db.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
        }
        await db.SaveChangesAsync();
        InvalidateCache();
    }

    public async Task SaveAllAsync(Dictionary<string, string> values)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        List<SiteSetting> existing = await db.SiteSettings.ToListAsync();
        Dictionary<string, SiteSetting> byKey = existing.ToDictionary(s => s.Key);
        foreach (KeyValuePair<string, string> kvp in values)
        {
            if (byKey.TryGetValue(kvp.Key, out SiteSetting? setting))
            {
                setting.Value = kvp.Value;
            }
            else
            {
                db.SiteSettings.Add(new SiteSetting { Key = kvp.Key, Value = kvp.Value });
            }
        }
        await db.SaveChangesAsync();
        InvalidateCache();
    }

    public void InvalidateCache() => cache.Remove(AllCacheKey);
}
