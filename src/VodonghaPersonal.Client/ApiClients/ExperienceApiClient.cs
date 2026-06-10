using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ExperienceApiClient(HttpClient http)
{
    public async Task<List<Experience>> GetAllAsync()
        => await http.GetFromJsonAsync<List<Experience>>("/api/admin/experience") ?? [];

    public async Task<Experience?> SaveAsync(Experience item)
    {
        HttpResponseMessage resp = item.Id == 0
            ? await http.PostAsJsonAsync("/api/admin/experience", item)
            : await http.PutAsJsonAsync($"/api/admin/experience/{item.Id}", item);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Experience>();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/experience/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
