using System.Net.Http.Headers;
using System.Text.Json;

namespace VodonghaPersonal.Services;

// ─── Data records ────────────────────────────────────────────────────────────

public record FlyMachine(
    string Id,
    string State,          // started | suspended | stopped
    string Region,
    string Size,           // e.g. shared-cpu-1x
    int CpuCount,
    int MemoryMb
);

public record FlyAppData(
    string AppName,
    List<FlyMachine> Machines,
    double ComputePerHour,        // USD, if running 1h
    double ComputePerMonth24h,    // USD, theoretical 24/7 max
    double Ipv4PerMonth,
    double FreeAllowance,
    double EstimatedBillable,     // after free allowance
    double EstimatedMtdDollars    // estimated month-to-date (days elapsed × daily rate)
);

public record NeonProjectData(
    string Name,
    string Plan,           // free | scale | launch
    string Region,
    long StorageBytes,
    double StorageMb,
    double StorageGb,
    int PgVersion,
    double EstimatedMonthlyCost
);

public record CostSummary(
    FlyAppData? Fly,
    NeonProjectData? Neon,
    DateTime FetchedAt
);

// ─── Service ─────────────────────────────────────────────────────────────────

public class CostMonitorService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly AppSecretsService _secrets;
    private readonly ILogger<CostMonitorService> _logger;

    private CostSummary? _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    // Fly.io pricing — shared-cpu-1x
    private const double FlySharedCpuPerSec   = 0.0000008;    // per vCPU-second
    private const double FlyRamPerMbPerSec     = 0.0000000128; // per MB-second
    private const double FlyIpv4PerMonth       = 2.00;
    private const double FlyFreeAllowance      = 5.00;         // monthly org credit

    // Neon pricing — Scale tier
    private const double NeonStoragePerGbMonth = 1.75;
    private const long   NeonFreeStorageBytes  = 512L * 1024 * 1024; // 512 MB

    public CostMonitorService(IHttpClientFactory httpFactory, AppSecretsService secrets, ILogger<CostMonitorService> logger)
    {
        _httpFactory = httpFactory;
        _secrets     = secrets;
        _logger      = logger;
    }

    public async Task<CostSummary> GetSummaryAsync()
    {
        if (_cache != null && DateTime.UtcNow - _cache.FetchedAt < CacheTtl)
        {
            return _cache;
        }

        FlyAppData? fly  = await FetchFlyAsync();
        NeonProjectData? neon = await FetchNeonAsync();

        _cache = new CostSummary(fly, neon, DateTime.UtcNow);
        return _cache;
    }

    public void InvalidateCache() => _cache = null;

    // ─── Fly.io ──────────────────────────────────────────────────────────────

    private async Task<FlyAppData?> FetchFlyAsync()
    {
        string? token   = await _secrets.GetValueAsync("Fly:ApiToken");
        string  appName = await _secrets.GetValueAsync("Fly:AppName") ?? "vodongha";

        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        try
        {
            using HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.Timeout = TimeSpan.FromSeconds(10);

            HttpResponseMessage response = await client.GetAsync(
                $"https://api.machines.dev/v1/apps/{appName}/machines");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Fly.io API returned {Status}", response.StatusCode);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            List<FlyMachine> machines = [];

            foreach (JsonElement el in doc.RootElement.EnumerateArray())
            {
                string id     = el.TryGetProperty("id",     out JsonElement eid)    ? eid.GetString()    ?? "" : "";
                string state  = el.TryGetProperty("state",  out JsonElement est)    ? est.GetString()    ?? "" : "";
                string region = el.TryGetProperty("region", out JsonElement ereg)   ? ereg.GetString()   ?? "" : "";

                int    cpus      = 1;
                int    memoryMb  = 256;
                string cpuKind   = "shared";

                if (el.TryGetProperty("config", out JsonElement cfg) &&
                    cfg.TryGetProperty("guest", out JsonElement guest))
                {
                    if (guest.TryGetProperty("cpus",      out JsonElement ec))  cpus     = ec.GetInt32();
                    if (guest.TryGetProperty("memory_mb", out JsonElement em))  memoryMb = em.GetInt32();
                    if (guest.TryGetProperty("cpu_kind",  out JsonElement ek))  cpuKind  = ek.GetString() ?? "shared";
                }

                string size = $"{cpuKind}-cpu-{cpus}x";
                machines.Add(new FlyMachine(id, state, region, size, cpus, memoryMb));
            }

            // Theoretical cost if all machines run 24/7 for 30 days
            double computePerHour = machines.Sum(m =>
                m.CpuCount * FlySharedCpuPerSec * 3600 +
                m.MemoryMb * FlyRamPerMbPerSec  * 3600);

            double computePerMonth = computePerHour * 24 * 30;
            double totalWithIpv4   = computePerMonth + FlyIpv4PerMonth;
            double billable        = Math.Max(0, totalWithIpv4 - FlyFreeAllowance);

            // Estimate MTD based on days elapsed in current month (no billing API needed)
            DateTime now = DateTime.UtcNow;
            double daysElapsed  = now.Day - 1 + now.Hour / 24.0; // days so far this month
            double dailyCompute = computePerMonth / 30.0;
            double dailyIpv4    = FlyIpv4PerMonth / 30.0;
            double rawMtd       = (dailyCompute + dailyIpv4) * daysElapsed;
            double estimatedMtd = Math.Max(0, rawMtd - FlyFreeAllowance);

            return new FlyAppData(appName, machines, computePerHour, computePerMonth,
                FlyIpv4PerMonth, FlyFreeAllowance, billable, estimatedMtd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Fly.io data");
            return null;
        }
    }

    // ─── Neon ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Neon's project object has store_bytes = 0 always.
    /// Real storage is the sum of logical_size across all branches.
    /// </summary>
    private async Task<long> FetchNeonStorageBytesAsync(HttpClient client, string projectId)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync(
                $"https://console.neon.tech/api/v2/projects/{projectId}/branches");

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("branches", out JsonElement branches))
            {
                return 0;
            }

            long total = 0;
            foreach (JsonElement branch in branches.EnumerateArray())
            {
                if (branch.TryGetProperty("logical_size", out JsonElement ls))
                {
                    total += ls.GetInt64();
                }
            }

            return total;
        }
        catch
        {
            return 0;
        }
    }

    private async Task<NeonProjectData?> FetchNeonAsync()
    {
        string? apiKey    = await _secrets.GetValueAsync("Neon:ApiKey");
        string? projectId = await _secrets.GetValueAsync("Neon:ProjectId");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(projectId))
        {
            return null;
        }

        try
        {
            using HttpClient client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.Timeout = TimeSpan.FromSeconds(10);

            HttpResponseMessage response = await client.GetAsync(
                $"https://console.neon.tech/api/v2/projects/{projectId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Neon API returned {Status}", response.StatusCode);
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement p = doc.RootElement.GetProperty("project");

            string name      = p.TryGetProperty("name",       out JsonElement en)  ? en.GetString()  ?? "" : "";
            string plan      = p.TryGetProperty("plan",       out JsonElement epl) ? epl.GetString() ?? "free" : "free";
            string region    = p.TryGetProperty("region_id",  out JsonElement er)  ? er.GetString()  ?? "" : "";
            int    pgVersion = p.TryGetProperty("pg_version", out JsonElement epg) ? epg.GetInt32()  : 16;

            // Fetch real storage from branches — project.store_bytes is always 0
            // Each branch has logical_size in bytes; sum all branches for total storage
            long storeBytes = await FetchNeonStorageBytesAsync(client, projectId);

            double storageMb = storeBytes / 1024.0 / 1024.0;
            double storageGb = storageMb  / 1024.0;

            double cost = plan == "free" ? 0 :
                Math.Max(0, (storeBytes - NeonFreeStorageBytes) / 1024.0 / 1024.0 / 1024.0 * NeonStoragePerGbMonth);

            return new NeonProjectData(name, plan, region, storeBytes, storageMb, storageGb, pgVersion, cost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Neon data");
            return null;
        }
    }
}
