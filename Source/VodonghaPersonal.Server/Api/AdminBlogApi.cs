using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminBlogApi
{
    public static void MapAdminBlogApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/blog").RequireAuthorization();

        group.MapGet("/", async (BlogService svc) =>
        {
            List<BlogPost> posts = await svc.GetAllAsync();
            return Results.Ok(posts);
        });

        group.MapPost("/", async (BlogPost post, BlogService svc) =>
        {
            await svc.SaveAsync(post);
            return Results.Ok(post);
        }).DisableAntiforgery();

        group.MapPut("/{id:guid}", async (Guid id, BlogPost post, BlogService svc) =>
        {
            post.Id = id;
            await svc.SaveAsync(post);
            return Results.Ok(post);
        }).DisableAntiforgery();

        group.MapDelete("/{id:guid}", async (Guid id, BlogService svc) =>
        {
            await svc.DeleteAsync(id);
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
