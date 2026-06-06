using Microsoft.EntityFrameworkCore;
using vodongha.Data;

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
}
