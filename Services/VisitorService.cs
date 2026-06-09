using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Data.Models;

namespace VodonghaPersonal.Services;

public class VisitorService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<int> GetCountAsync()
    {
        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        return await db.VisitorLogs.CountAsync();
    }

    public async Task LogAsync(string ip, string? userAgent = null)
    {
        // Skip localhost and private addresses
        if (string.IsNullOrWhiteSpace(ip) || ip == "::1" || ip.StartsWith("127.") || ip.StartsWith("10.") || ip == "unknown")
        {
            return;
        }

        await using AppDbContext db = await dbFactory.CreateDbContextAsync();
        bool exists = await db.VisitorLogs.AnyAsync(v => v.IpAddress == ip);
        if (!exists)
        {
            db.VisitorLogs.Add(new VisitorLog { IpAddress = ip, FirstSeenAt = DateTime.UtcNow, UserAgent = userAgent });
            try { await db.SaveChangesAsync(); } catch { /* ignore duplicate key on race */ }
        }
    }
}
