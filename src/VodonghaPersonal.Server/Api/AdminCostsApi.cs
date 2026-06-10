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

            FlyAppDto? fly = summary.Fly is null ? null : new FlyAppDto(
                summary.Fly.AppName,
                summary.Fly.Machines.Select(m => new FlyMachineDto(m.Id, m.State, m.Region, m.Size, m.CpuCount, m.MemoryMb)).ToList(),
                summary.Fly.ComputePerHour,
                summary.Fly.ComputePerMonth24h,
                summary.Fly.Ipv4PerMonth,
                summary.Fly.FreeAllowance,
                summary.Fly.EstimatedBillable,
                summary.Fly.EstimatedMtdDollars
            );

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

            return Results.Ok(new CostSummaryDto(fly, neon, summary.FetchedAt));
        });

        group.MapPost("/invalidate", (CostMonitorService svc) =>
        {
            svc.InvalidateCache();
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
