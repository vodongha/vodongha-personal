using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Services;

public class AnalyticsService(IDbContextFactory<AppDbContext> dbFactory, IHttpClientFactory httpClientFactory, ILogger<AnalyticsService> logger)
{
    // Static cache survives across request scopes; keyed by IP
    private static readonly ConcurrentDictionary<string, (string Country, DateTime ExpiresAt)> _geoCache = new();

    public async Task TrackAsync(string path, string? referrer, string ip)
    {
        try
        {
            // Await geo lookup directly — TrackAsync is already called fire-and-forget from middleware
            string? country = null;
            if (_geoCache.TryGetValue(ip, out (string Country, DateTime ExpiresAt) cached) && cached.ExpiresAt > DateTime.UtcNow)
            {
                country = cached.Country;
            }
            else if (IsPublicIp(ip))
            {
                country = await LookupAndCacheAsync(ip);
            }

            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            db.PageViews.Add(new PageView
            {
                Path = path,
                Referrer = CleanReferrer(referrer),
                Country = country,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Analytics: failed to track {Path}", path);
        }
    }

    public async Task<List<(DateTime Date, int Count)>> GetDailyViewsAsync(int days = 30)
    {
        DateTime since = DateTime.UtcNow.Date.AddDays(-days + 1);
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();

        var raw = await db.PageViews
            .Where(p => p.CreatedAt >= since)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        List<(DateTime, int)> result = [];
        for (int i = 0; i < days; i++)
        {
            DateTime date = since.AddDays(i);
            int count = raw.FirstOrDefault(r => r.Date == date)?.Count ?? 0;
            result.Add((date, count));
        }
        return result;
    }

    public async Task<List<(string Path, int Count)>> GetTopPagesAsync(int days = 30, int limit = 10)
    {
        DateTime since = DateTime.UtcNow.AddDays(-days);
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        var rows = await db.PageViews
            .Where(p => p.CreatedAt >= since)
            .GroupBy(p => p.Path)
            .Select(g => new { Path = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();
        return rows.Select(x => (x.Path, x.Count)).ToList();
    }

    public async Task<List<(string Referrer, int Count)>> GetTopReferrersAsync(int days = 30, int limit = 10)
    {
        DateTime since = DateTime.UtcNow.AddDays(-days);
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        var rows = await db.PageViews
            .Where(p => p.CreatedAt >= since && p.Referrer != null)
            .GroupBy(p => p.Referrer!)
            .Select(g => new { Referrer = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();
        return rows.Select(x => (x.Referrer, x.Count)).ToList();
    }

    public async Task<List<(string Country, int Count)>> GetTopCountriesAsync(int days = 30, int limit = 10)
    {
        DateTime since = DateTime.UtcNow.AddDays(-days);
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        var rows = await db.PageViews
            .Where(p => p.CreatedAt >= since && p.Country != null)
            .GroupBy(p => p.Country!)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(limit)
            .ToListAsync();
        return rows.Select(x => (x.Country, x.Count)).ToList();
    }

    public async Task<int> GetTotalAsync(int days)
    {
        DateTime since = days == 0 ? DateTime.MinValue : DateTime.UtcNow.AddDays(-days);
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.PageViews.CountAsync(p => days == 0 || p.CreatedAt >= since);
    }

    private async Task<string?> LookupAndCacheAsync(string ip)
    {
        try
        {
            using HttpClient http = httpClientFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(3);
            GeoResponse? response = await http.GetFromJsonAsync<GeoResponse>($"http://ip-api.com/json/{ip}?fields=country");
            string country = response?.Country ?? "Unknown";
            _geoCache[ip] = (country, DateTime.UtcNow.AddHours(24));
            return country;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsPublicIp(string ip)
    {
        if (string.IsNullOrEmpty(ip) || ip == "unknown" || ip == "::1" || ip == "127.0.0.1")
        {
            return false;
        }
        return !ip.StartsWith("10.") && !ip.StartsWith("192.168.") && !ip.StartsWith("172.");
    }

    private static string? CleanReferrer(string? referrer)
    {
        if (string.IsNullOrWhiteSpace(referrer))
        {
            return null;
        }
        try
        {
            Uri uri = new(referrer);
            return uri.Host.ToLowerInvariant();
        }
        catch
        {
            return null;
        }
    }

    private sealed record GeoResponse([property: JsonPropertyName("country")] string? Country);
}
