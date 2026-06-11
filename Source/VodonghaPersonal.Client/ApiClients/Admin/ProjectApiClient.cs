using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ProjectApiClient(HttpClient http) : BaseCrudApiClient<Project>(http, "/api/admin/projects")
{
    public Task<Project?> SaveAsync(Project item) => SaveAsync(item, item.Rid);

    public async Task SaveOrderAsync(List<Guid> rids)
    {
        HttpResponseMessage resp = await Http.PutAsJsonAsync("/api/admin/projects/order", new OrderUpdateRequest(rids));
        resp.EnsureSuccessStatusCode();
    }
}
