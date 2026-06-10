using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ProjectApiClient(HttpClient http)
{
    public async Task<List<Project>> GetAllAsync()
        => await http.GetFromJsonAsync<List<Project>>("/api/admin/projects") ?? [];

    public async Task<Project?> SaveAsync(Project item)
    {
        HttpResponseMessage resp = item.Id == 0
            ? await http.PostAsJsonAsync("/api/admin/projects", item)
            : await http.PutAsJsonAsync($"/api/admin/projects/{item.Id}", item);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<Project>();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/projects/{id}");
        resp.EnsureSuccessStatusCode();
    }

    public async Task SaveOrderAsync(List<int> ids)
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync("/api/admin/projects/order", new OrderUpdateRequest(ids));
        resp.EnsureSuccessStatusCode();
    }
}
