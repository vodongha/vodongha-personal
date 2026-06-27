using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ContactApiClient(HttpClient http)
{
    public async Task<List<ContactMessage>> GetAllAsync()
        => await http.GetFromJsonAsync<List<ContactMessage>>("/api/admin/contacts") ?? [];

    public async Task<PagedResult<ContactMessage>> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null)
    {
        string url = $"/api/admin/contacts/paged?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) { url += $"&search={Uri.EscapeDataString(search)}"; }
        if (!string.IsNullOrEmpty(sortBy)) { url += $"&sortBy={Uri.EscapeDataString(sortBy)}&sortDir={sortDir ?? "asc"}"; }
        return await http.GetFromJsonAsync<PagedResult<ContactMessage>>(url) ?? new PagedResult<ContactMessage>([], 0);
    }

    public async Task<int> GetUnreadCountAsync()
        => await http.GetFromJsonAsync<int>("/api/admin/contacts/unread-count");

    public async Task MarkReadAsync(Guid rid)
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync($"/api/admin/contacts/{rid}/read", new { });
        resp.EnsureSuccessStatusCode();
    }

    public async Task MarkAllReadAsync()
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync("/api/admin/contacts/read-all", new { });
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid rid)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/contacts/{rid}");
        resp.EnsureSuccessStatusCode();
    }
}
