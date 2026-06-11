using System.Net.Http.Json;

namespace VodonghaPersonal.Client.ApiClients;

public class PublicSiteApiClient(HttpClient http)
{
    public async Task<int> GetVisitorCountAsync()
        => await http.GetFromJsonAsync<int>("/api/site/visitor-count");

    public async Task<Dictionary<string, string>> GetSettingsAsync()
        => await http.GetFromJsonAsync<Dictionary<string, string>>("/api/site/settings") ?? [];
}
