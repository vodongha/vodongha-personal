using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.ApiClients;

public abstract class BaseCrudApiClient<T>(HttpClient http, string route) where T : class
{
    protected readonly HttpClient Http = http;

    public async Task<List<T>> GetAllAsync()
        => await Http.GetFromJsonAsync<List<T>>(route) ?? [];

    public async Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, string? search = null, string? sortBy = null, string? sortDir = null)
    {
        string url = $"{route}/paged?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) { url += $"&search={Uri.EscapeDataString(search)}"; }
        if (!string.IsNullOrEmpty(sortBy)) { url += $"&sortBy={Uri.EscapeDataString(sortBy)}&sortDir={sortDir ?? "asc"}"; }
        return await Http.GetFromJsonAsync<PagedResult<T>>(url) ?? new PagedResult<T>([], 0);
    }

    public async Task<T?> SaveAsync(T item, Guid rid)
    {
        HttpResponseMessage resp = rid == Guid.Empty
            ? await Http.PostAsJsonAsync(route, item)
            : await Http.PutAsJsonAsync($"{route}/{rid}", item);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    public async Task DeleteAsync(Guid rid)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"{route}/{rid}");
        resp.EnsureSuccessStatusCode();
    }
}
