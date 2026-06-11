using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class SiteSettingService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public string Get(Dictionary<string, string> settings, string key, string fallback = "")
        => settings.TryGetValue(key, out string? val) ? val : fallback;

    public async Task<string?> GetAsync(string key)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        SiteSetting? setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
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
    }

    /// <summary>Upserts multiple settings in a single DB round-trip.</summary>
    public async Task SaveAllAsync(Dictionary<string, string> values)
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        foreach (KeyValuePair<string, string> kvp in values)
        {
            SiteSetting? setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == kvp.Key);
            if (setting != null)
            {
                setting.Value = kvp.Value;
            }
            else
            {
                db.SiteSettings.Add(new SiteSetting { Key = kvp.Key, Value = kvp.Value });
            }
        }
        await db.SaveChangesAsync();
    }
}
