using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class PublicBlogApiClient(HttpClient http)
{
    public async Task<List<BlogPost>> GetPublishedAsync()
        => await http.GetFromJsonAsync<List<BlogPost>>("/api/blog/published") ?? [];

    public async Task<BlogPost?> GetBySlugAsync(string slug)
        => await http.GetFromJsonAsync<BlogPost>($"/api/blog/{slug}");

    public async Task IncrementViewAsync(int id)
    {
        try
        {
            await http.PostAsync($"/api/blog/{id}/view", null);
        }
        catch
        {
            // fire and forget — view count failure is non-critical
        }
    }
}
