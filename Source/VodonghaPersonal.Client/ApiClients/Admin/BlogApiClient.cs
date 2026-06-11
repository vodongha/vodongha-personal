using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class BlogApiClient(HttpClient http) : BaseCrudApiClient<BlogPost>(http, "/api/admin/blog")
{
    public Task<BlogPost?> SaveAsync(BlogPost post) => SaveAsync(post, post.Rid);
}
