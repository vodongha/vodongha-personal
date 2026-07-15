using System.Net.Http.Headers;
using System.Text.Json;

namespace VodonghaPersonal.Services;

// ─── Data records ────────────────────────────────────────────────────────────

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

    // Neon pricing — Scale tier
    private const double NeonStoragePerGbMonth = 1.75;
    private const long NeonFreeStorageBytes = 512L * 1024 * 1024; // 512 MB

    public CostMonitorService(IHttpClientFactory httpFactory, AppSecretsService secrets, ILogger<CostMonitorService> logger)
    {
        _httpFactory = httpFactory;
        _secrets = secrets;
        _logger = logger;
    }

    public async Task<CostSummary> GetSummaryAsync()
    {
        if (_cache != null && DateTime.UtcNow - _cache.FetchedAt < CacheTtl)
        {
            return _cache;
        }

        NeonProjectData? neon = await FetchNeonAsync();

        _cache = new CostSummary(neon, DateTime.UtcNow);
        return _cache;
    }

    public void InvalidateCache() => _cache = null;

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
        string? apiKey = await _secrets.GetValueAsync("Neon:ApiKey");
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

            string name = p.TryGetProperty("name", out JsonElement en) ? en.GetString() ?? "" : "";
            string plan = p.TryGetProperty("plan", out JsonElement epl) ? epl.GetString() ?? "free" : "free";
            string region = p.TryGetProperty("region_id", out JsonElement er) ? er.GetString() ?? "" : "";
            int pgVersion = p.TryGetProperty("pg_version", out JsonElement epg) ? epg.GetInt32() : 16;

            // Fetch real storage from branches — project.store_bytes is always 0
            // Each branch has logical_size in bytes; sum all branches for total storage
            long storeBytes = await FetchNeonStorageBytesAsync(client, projectId);

            double storageMb = storeBytes / 1024.0 / 1024.0;
            double storageGb = storageMb / 1024.0;

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
