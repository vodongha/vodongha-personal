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
            db.Projects.Update(item);
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
    }
}
