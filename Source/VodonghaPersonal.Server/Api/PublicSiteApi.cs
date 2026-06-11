using VodonghaPersonal.Services;

namespace VodonghaPersonal.Api;

public static class PublicSiteApi
{
    public static void MapPublicSiteApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/site");

        group.MapGet("/visitor-count", async (VisitorService svc) =>
        {
            int count = await svc.GetCountAsync();
            return Results.Ok(count);
        });

        group.MapGet("/settings", async (SiteSettingService svc) =>
        {
            Dictionary<string, string> settings = await svc.GetAllAsync();
            return Results.Ok(settings);
        });
    }
}
