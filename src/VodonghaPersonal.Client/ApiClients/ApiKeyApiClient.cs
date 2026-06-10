using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public class ApiKeyApiClient(HttpClient http)
{
    public async Task<ApiKeysPageDto?> GetAllAsync()
        => await http.GetFromJsonAsync<ApiKeysPageDto>("/api/admin/api-keys");

    public async Task SaveAsync(string key, string value)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync("/api/admin/api-keys", new ApiKeyDto(key, value));
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string key)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/api-keys/{Uri.EscapeDataString(key)}");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var result = await http.GetFromJsonAsync<ValueResult>($"/api/admin/api-keys/{Uri.EscapeDataString(key)}/value");
        return result?.Value;
    }

    private record ValueResult(string Value);
}
