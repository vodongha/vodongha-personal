using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminAnalyticsApi
{
    public static void MapAdminAnalyticsApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/analytics", async (AnalyticsService svc, int days = 30) =>
        {
            int[] dayValues = days == 0 ? [0] : [];

            List<Task> tasks = [];
            int total = 0, totalAll = 0;
            List<(string Path, int Count)> topPages = [];
            List<(string Country, int Count)> topCountries = [];
            List<(string Referrer, int Count)> topReferrers = [];
            List<(DateTime Date, int Count)> daily = [];

            await Task.WhenAll(
                Task.Run(async () => { total = await svc.GetTotalAsync(days); }),
                Task.Run(async () => { totalAll = await svc.GetTotalAsync(0); }),
                Task.Run(async () => { topPages = await svc.GetTopPagesAsync(days); }),
                Task.Run(async () => { topCountries = await svc.GetTopCountriesAsync(days); }),
                Task.Run(async () => { topReferrers = await svc.GetTopReferrersAsync(days); }),
                Task.Run(async () => { daily = await svc.GetDailyViewsAsync(days); })
            );

            AnalyticsDto dto = new(
                Total: total,
                TotalAll: totalAll,
                TopPages: topPages.Select(x => new TopItemDto(x.Path, x.Count)).ToList(),
                TopCountries: topCountries.Select(x => new TopItemDto(x.Country, x.Count)).ToList(),
                TopReferrers: topReferrers.Select(x => new TopItemDto(x.Referrer, x.Count)).ToList(),
                Daily: daily.Select(x => new DailyViewDto(x.Date, x.Count)).ToList()
            );

            return Results.Ok(dto);
        }).RequireAuthorization();
    }
}
