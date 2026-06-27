using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminProjectsApi
{
    public static void MapAdminProjectsApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/projects").RequireAuthorization();

        group.MapGet("/", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            List<Project> items = await db.Projects.OrderBy(p => p.Order).ToListAsync();
            return Results.Ok(items);
        });

        group.MapGet("/paged", async (int? page, int? pageSize, string? search, string? sortBy, string? sortDir, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            IQueryable<Project> q = db.Projects;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = $"%{search.Trim()}%";
                q = q.Where(x => EF.Functions.ILike(x.Title, s) || EF.Functions.ILike(x.Technologies, s));
            }
            bool desc = sortDir == "desc";
            q = sortBy switch
            {
                "Title" => desc ? q.OrderByDescending(x => x.Title) : q.OrderBy(x => x.Title),
                _ => q.OrderBy(x => x.Order).ThenBy(x => x.Id)
            };
            return Results.Ok(await q.ToPagedResultAsync(page ?? 0, pageSize ?? 10));
        });

        group.MapPost("/", async (Project item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = 0;
            db.Projects.Add(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            projectSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapPut("/{rid:guid}", async (Guid rid, Project item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Project? existing = await db.Projects.FirstOrDefaultAsync(p => p.Rid == rid);
            if (existing is null) { return Results.NotFound(); }
            item.Id = existing.Id;
            item.Rid = rid;
            db.Entry(existing).CurrentValues.SetValues(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            projectSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Project? p = await db.Projects.FirstOrDefaultAsync(x => x.Rid == rid);
            if (p != null)
            {
                db.Projects.Remove(p);
                await db.SaveChangesAsync();
                cvCache.InvalidateAndRegenerate();
                projectSvc.InvalidateCache();
            }
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapPut("/order", async (OrderUpdateRequest req, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            for (int i = 0; i < req.Rids.Count; i++)
            {
                Guid rid = req.Rids[i];
                await db.Projects.Where(x => x.Rid == rid).ExecuteUpdateAsync(s => s.SetProperty(x => x.Order, i + 1));
            }
            cvCache.InvalidateAndRegenerate();
            projectSvc.InvalidateCache();
            return Results.Ok();
        }).DisableAntiforgery();

        // Within-page reorder for the server-side table: apply explicit (rid, order) pairs
        // so only the dragged page's Order slots are permuted (global order stays intact).
        group.MapPut("/reorder", async (List<ProjectOrderItem> items, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            foreach (ProjectOrderItem it in items)
            {
                await db.Projects.Where(x => x.Rid == it.Rid).ExecuteUpdateAsync(s => s.SetProperty(x => x.Order, it.Order));
            }
            cvCache.InvalidateAndRegenerate();
            projectSvc.InvalidateCache();
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
