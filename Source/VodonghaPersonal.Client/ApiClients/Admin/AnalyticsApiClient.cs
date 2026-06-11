using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class AnalyticsApiClient(HttpClient http)
{
    public async Task<AnalyticsDto?> GetAsync(int days = 30)
        => await http.GetFromJsonAsync<AnalyticsDto>($"/api/admin/analytics?days={days}");
}
