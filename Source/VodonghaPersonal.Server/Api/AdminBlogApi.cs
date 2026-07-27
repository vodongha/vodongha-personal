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

        group.MapGet("/paged", async (int? page, int? pageSize, string? search, string? sortBy, string? sortDir, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            IQueryable<BlogPost> q = db.BlogPosts;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = $"%{search.Trim()}%";
                q = q.Where(x => EF.Functions.Like(x.Title.ToUpper(), s.ToUpper()) || EF.Functions.Like(x.Slug.ToUpper(), s.ToUpper()) || EF.Functions.Like(x.Tags.ToUpper(), s.ToUpper()));
            }
            bool desc = sortDir == "desc";
            q = sortBy switch
            {
                "Title" => desc ? q.OrderByDescending(x => x.Title) : q.OrderBy(x => x.Title),
                "Slug" => desc ? q.OrderByDescending(x => x.Slug) : q.OrderBy(x => x.Slug),
                "ViewCount" => desc ? q.OrderByDescending(x => x.ViewCount) : q.OrderBy(x => x.ViewCount),
                "IsPublished" => desc ? q.OrderByDescending(x => x.IsPublished) : q.OrderBy(x => x.IsPublished),
                "CreatedAt" => desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt),
                _ => q.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            };
            return Results.Ok(await q.ToPagedResultAsync(page ?? 0, pageSize ?? 10));
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
