using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminDashboardApi
{
    public static void MapAdminDashboardApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/dashboard", async (
            IDbContextFactory<AppDbContext> dbFactory,
            ChatService chatSvc) =>
        {
            DateTime now = DateTime.UtcNow;
            DateTime cutoff30 = now.AddDays(-30);
            DateTime cutoff14 = now.AddDays(-14);

            await using AppDbContext db = await dbFactory.CreateDbContextAsync();

            int totalVisitors = await db.VisitorLogs.CountAsync();
            int pageViews30d = await db.PageViews.CountAsync(p => p.CreatedAt >= cutoff30);
            int totalBlogViews = await db.BlogPosts.SumAsync(b => (int?)b.ViewCount) ?? 0;
            int unreadMessages = await db.ContactMessages.CountAsync(m => !m.IsRead);
            int unreadChats = await chatSvc.GetUnreadCountAsync();
            int skillCount = await db.Skills.CountAsync();
            int projectCount = await db.Projects.CountAsync();
            int blogCount = await db.BlogPosts.CountAsync();
            int expCount = await db.Experiences.CountAsync();
            int eduCount = await db.Educations.CountAsync();

            var skillsByCategory = await db.Skills
                .GroupBy(s => s.Category)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var blogViews = await db.BlogPosts
                .OrderByDescending(b => b.ViewCount)
                .Take(6)
                .Select(b => new { Label = b.TitleEn ?? b.Title, Count = b.ViewCount })
                .ToListAsync();

            var trend = await db.PageViews
                .Where(p => p.CreatedAt >= cutoff14)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToListAsync();

            List<string> trendLabels = [];
            List<int> trendData = [];
            for (int i = 0; i < 14; i++)
            {
                DateTime day = cutoff14.Date.AddDays(i);
                trendLabels.Add(day.ToString("MM/dd"));
                trendData.Add(trend.FirstOrDefault(t => t.Date == day)?.Count ?? 0);
            }

            var recentContacts = await db.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .Take(5)
                .Select(m => new RecentContactDto(
                    m.Name,
                    m.Message.Length > 60 ? m.Message.Substring(0, 58) + "…" : m.Message,
                    m.SentAt,
                    m.IsRead))
                .ToListAsync();

            DashboardStatsDto stats = new(
                TotalVisitors: totalVisitors,
                PageViews30d: pageViews30d,
                TotalBlogViews: totalBlogViews,
                UnreadMessages: unreadMessages,
                UnreadChats: unreadChats,
                SkillCount: skillCount,
                ProjectCount: projectCount,
                BlogCount: blogCount,
                ExpCount: expCount,
                EduCount: eduCount,
                SkillCatLabels: skillsByCategory.Select(x => x.Label).ToList(),
                SkillCatData: skillsByCategory.Select(x => x.Count).ToList(),
                BlogLabels: blogViews.Select(x => x.Label).ToList(),
                BlogData: blogViews.Select(x => x.Count).ToList(),
                TrendLabels: trendLabels,
                TrendData: trendData,
                RecentContacts: recentContacts
            );

            return Results.Ok(stats);
        }).RequireAuthorization();
    }
}
