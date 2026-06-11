using System.Net.Http.Json;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class PublicBlogApiClient(HttpClient http)
{
    public async Task<List<BlogPost>> GetPublishedAsync()
        => await http.GetFromJsonAsync<List<BlogPost>>("/api/blog/published") ?? [];

    public async Task<BlogPost?> GetBySlugAsync(string slug)
        => await http.GetFromJsonAsync<BlogPost>($"/api/blog/{slug}");

    public async Task<List<BlogPost>> GetRelatedAsync(Guid rid, string tags)
        => await http.GetFromJsonAsync<List<BlogPost>>($"/api/blog/{rid}/related?tags={Uri.EscapeDataString(tags)}") ?? [];

    public async Task IncrementViewAsync(Guid rid)
    {
        try
        {
            await http.PostAsync($"/api/blog/{rid}/view", null);
        }
        catch
        {
            // fire and forget — view count failure is non-critical
        }
    }
}
