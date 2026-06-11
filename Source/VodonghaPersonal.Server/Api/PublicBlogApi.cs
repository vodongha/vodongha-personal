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
            if (post is null) { return Results.NotFound(); }
            return Results.Ok(post);
        });

        group.MapGet("/{rid:guid}/related", async (Guid rid, string? tags, BlogService svc) =>
        {
            List<BlogPost> related = await svc.GetRelatedAsync(rid, tags ?? string.Empty);
            return Results.Ok(related);
        });

        group.MapPost("/{rid:guid}/view", async (Guid rid, BlogService svc) =>
        {
            _ = svc.IncrementViewCountAsync(rid);
            return Results.Ok();
        });
    }
}
