using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class BlogApiClient(HttpClient http)
{
    public async Task<List<BlogPost>> GetAllAsync()
        => await http.GetFromJsonAsync<List<BlogPost>>("/api/admin/blog") ?? [];

    public async Task<BlogPost?> SaveAsync(BlogPost post)
    {
        HttpResponseMessage resp = post.Id == 0
            ? await http.PostAsJsonAsync("/api/admin/blog", post)
            : await http.PutAsJsonAsync($"/api/admin/blog/{post.Id}", post);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<BlogPost>();
    }

    public async Task DeleteAsync(int id)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/blog/{id}");
        resp.EnsureSuccessStatusCode();
    }
}
