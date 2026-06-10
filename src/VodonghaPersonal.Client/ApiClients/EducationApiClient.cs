using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class EducationApiClient(HttpClient http)
{
    public async Task<List<Education>> GetAllAsync()
        => await http.GetFromJsonAsync<List<Education>>("/api/admin/education") ?? [];

    public async Task<Education?> SaveAsync(Education item)
    {
        HttpResponseMessage resp = item.Id == 0
            ? await http.PostAsJsonAsync("/api/admin/education", item)
            : await http.PutAsJsonAsync($"/api/admin/education/{item.Id}", item);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Education>();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/education/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
