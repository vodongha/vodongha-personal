using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class CostApiClient(HttpClient http)
{
    public async Task<CostSummaryDto?> GetAsync()
        => await http.GetFromJsonAsync<CostSummaryDto>("/api/admin/costs");

    public async Task InvalidateCacheAsync()
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync("/api/admin/costs/invalidate", new { });
        resp.EnsureSuccessStatusCode();
    }
}
