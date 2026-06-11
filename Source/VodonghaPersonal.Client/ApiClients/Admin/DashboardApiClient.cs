using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class DashboardApiClient(HttpClient http)
{
    public async Task<DashboardStatsDto?> GetStatsAsync()
        => await http.GetFromJsonAsync<DashboardStatsDto>("/api/admin/dashboard");
}
