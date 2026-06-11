using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminAnalyticsApi
{
    public static void MapAdminAnalyticsApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/analytics", async (AnalyticsService svc, int days = 30) =>
        {
            int total = await svc.GetTotalAsync(days);
            int totalAll = await svc.GetTotalAsync(0);
            List<(string Path, int Count)> topPages = await svc.GetTopPagesAsync(days);
            List<(string Country, int Count)> topCountries = await svc.GetTopCountriesAsync(days);
            List<(string Referrer, int Count)> topReferrers = await svc.GetTopReferrersAsync(days);
            List<(DateTime Date, int Count)> daily = await svc.GetDailyViewsAsync(days);

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
