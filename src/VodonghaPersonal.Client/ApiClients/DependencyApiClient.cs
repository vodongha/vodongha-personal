using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class DependencyApiClient(HttpClient http)
{
    public async Task<List<DependencyDto>> GetAllAsync()
        => await http.GetFromJsonAsync<List<DependencyDto>>("/api/admin/dependencies") ?? [];

    public async Task InvalidateCacheAsync()
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync("/api/admin/dependencies/invalidate", new { });
        resp.EnsureSuccessStatusCode();
    }
}
