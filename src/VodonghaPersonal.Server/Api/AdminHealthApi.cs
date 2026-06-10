using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminHealthApi
{
    public static void MapAdminHealthApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/health-metrics", (HealthMonitorService monitor) =>
        {
            List<HealthSnapshotDto> snapshots = monitor.GetSnapshots()
                .Select(s => new HealthSnapshotDto(s.Timestamp, s.MemoryMb, s.DbPingMs, s.ThreadCount, s.DbHealthy))
                .ToList();

            HealthMetricSnapshot? latest = monitor.Latest;
            HealthSnapshotDto? latestDto = latest != null
                ? new HealthSnapshotDto(latest.Timestamp, latest.MemoryMb, latest.DbPingMs, latest.ThreadCount, latest.DbHealthy)
                : null;

            HealthDataDto dto = new(
                Snapshots: snapshots,
                Latest: latestDto,
                UptimeSeconds: (long)monitor.Uptime.TotalSeconds,
                StartedAt: monitor.StartedAt
            );

            return Results.Ok(dto);
        }).RequireAuthorization();
    }
}
