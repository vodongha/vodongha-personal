using System.Net.Http.Json;

namespace VodonghaPersonal.Client.ApiClients;

public abstract class BaseCrudApiClient<T>(HttpClient http, string route) where T : class
{
    protected readonly HttpClient Http = http;

    public async Task<List<T>> GetAllAsync()
        => await Http.GetFromJsonAsync<List<T>>(route) ?? [];

    public async Task<T?> SaveAsync(T item, int id)
    {
        HttpResponseMessage resp = id == 0
            ? await Http.PostAsJsonAsync(route, item)
            : await Http.PutAsJsonAsync($"{route}/{id}", item);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<T>();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await Http.DeleteAsync($"{route}/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
