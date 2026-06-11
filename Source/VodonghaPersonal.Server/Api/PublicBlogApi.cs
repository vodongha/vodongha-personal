using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class PublicBlogApi
{
    public static void MapPublicBlogApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/blog");

        group.MapGet("/published", async (BlogService svc) =>
        {
            List<BlogPost> posts = await svc.GetPublishedAsync();
            return Results.Ok(posts);
        });

        group.MapGet("/{slug}", async (string slug, BlogService svc) =>
        {
            BlogPost? post = await svc.GetBySlugAsync(slug);
            if (post is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(post);
        });

        group.MapPost("/{id:int}/view", async (int id, BlogService svc) =>
        {
            _ = svc.IncrementViewCountAsync(id);
            return Results.Ok();
        });
    }
}
