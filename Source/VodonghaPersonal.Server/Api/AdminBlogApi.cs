using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
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

        group.MapPut("/{rid:guid}", async (Guid rid, BlogPost post, IDbContextFactory<AppDbContext> dbFactory, BlogService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            BlogPost? existing = await db.BlogPosts.FirstOrDefaultAsync(b => b.Rid == rid);
            if (existing is null) { return Results.NotFound(); }
            post.Id = existing.Id;
            post.Rid = rid;
            await svc.SaveAsync(post);
            return Results.Ok(post);
        }).DisableAntiforgery();

        group.MapDelete("/{rid:guid}", async (Guid rid, BlogService svc) =>
        {
            await svc.DeleteAsync(rid);
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
