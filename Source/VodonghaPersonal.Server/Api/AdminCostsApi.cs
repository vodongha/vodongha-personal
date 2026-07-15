using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminCostsApi
{
    public static void MapAdminCostsApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/costs").RequireAuthorization();

        group.MapGet("/", async (CostMonitorService svc) =>
        {
            CostSummary summary = await svc.GetSummaryAsync();

            NeonProjectDto? neon = summary.Neon is null ? null : new NeonProjectDto(
                summary.Neon.Name,
                summary.Neon.Plan,
                summary.Neon.Region,
                summary.Neon.StorageBytes,
                summary.Neon.StorageMb,
                summary.Neon.StorageGb,
                summary.Neon.PgVersion,
                summary.Neon.EstimatedMonthlyCost
            );

            return Results.Ok(new CostSummaryDto(neon, summary.FetchedAt));
        });

        group.MapPost("/invalidate", (CostMonitorService svc) =>
        {
            svc.InvalidateCache();
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
