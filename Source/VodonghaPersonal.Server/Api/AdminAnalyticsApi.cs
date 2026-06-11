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

            Task<int> totalTask = svc.GetTotalAsync(days);
            Task<int> totalAllTask = svc.GetTotalAsync(0);
            Task<List<(string Path, int Count)>> topPagesTask = svc.GetTopPagesAsync(days);
            Task<List<(string Country, int Count)>> topCountriesTask = svc.GetTopCountriesAsync(days);
            Task<List<(string Referrer, int Count)>> topReferrersTask = svc.GetTopReferrersAsync(days);
            Task<List<(DateTime Date, int Count)>> dailyTask = svc.GetDailyViewsAsync(days);

            await Task.WhenAll(totalTask, totalAllTask, topPagesTask, topCountriesTask, topReferrersTask, dailyTask);

            AnalyticsDto dto = new(
                Total: await totalTask,
                TotalAll: await totalAllTask,
                TopPages: (await topPagesTask).Select(x => new TopItemDto(x.Path, x.Count)).ToList(),
                TopCountries: (await topCountriesTask).Select(x => new TopItemDto(x.Country, x.Count)).ToList(),
                TopReferrers: (await topReferrersTask).Select(x => new TopItemDto(x.Referrer, x.Count)).ToList(),
                Daily: (await dailyTask).Select(x => new DailyViewDto(x.Date, x.Count)).ToList()
            );

            return Results.Ok(dto);
        }).RequireAuthorization();
    }
}
