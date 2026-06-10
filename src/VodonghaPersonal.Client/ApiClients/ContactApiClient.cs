using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ContactApiClient(HttpClient http)
{
    public async Task<List<ContactMessage>> GetAllAsync()
        => await http.GetFromJsonAsync<List<ContactMessage>>("/api/admin/contacts") ?? [];

    public async Task MarkReadAsync(int id)
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync($"/api/admin/contacts/{id}/read", new { });
        resp.EnsureSuccessStatusCode();
    }

    public async Task MarkAllReadAsync()
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync("/api/admin/contacts/read-all", new { });
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/contacts/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
