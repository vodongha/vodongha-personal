using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;

namespace vodongha.Services;

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
}
