using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class HealthApiClient(HttpClient http)
{
    public async Task<HealthDataDto?> GetAsync()
        => await http.GetFromJsonAsync<HealthDataDto>("/api/admin/health-metrics");
}
