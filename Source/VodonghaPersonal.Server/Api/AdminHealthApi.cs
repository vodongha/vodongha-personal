using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

public static class AdminHealthApi
{
    private const string VersionCacheKey = "github_latest_version";

    public static void MapAdminHealthApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/version", async (IMemoryCache cache, IHttpClientFactory httpFactory) =>
        {
            if (cache.TryGetValue(VersionCacheKey, out string? cached))
            {
                return Results.Ok(new { version = cached });
            }

            try
            {
                HttpClient http = httpFactory.CreateClient("github");
                GitHubRelease? release = await http.GetFromJsonAsync<GitHubRelease>(
                    "https://api.github.com/repos/vodongha/vodongha-personal/releases/latest");
                string version = release?.TagName ?? "—";
                cache.Set(VersionCacheKey, version, TimeSpan.FromHours(1));
                return Results.Ok(new { version });
            }
            catch
            {
                return Results.Ok(new { version = ResolveAppVersion() });
            }
        }).RequireAuthorization();

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
                StartedAt: monitor.StartedAt,
                AppVersion: ResolveAppVersion()
            );

            return Results.Ok(dto);
        }).RequireAuthorization();
    }

    private static string ResolveAppVersion()
    {
        string? assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3);
        if (assemblyVersion is not null && assemblyVersion != "0.0.0" && assemblyVersion != "1.0.0")
        {
            return assemblyVersion;
        }
        return Environment.GetEnvironmentVariable("APP_VERSION") ?? "unknown";
    }

    private record GitHubRelease([property: JsonPropertyName("tag_name")] string? TagName);
}
