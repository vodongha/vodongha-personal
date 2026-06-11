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

        group.MapPut("/{id:int}", async (int id, Project item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = id;
            db.Projects.Update(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            projectSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{id:int}", async (int id, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ProjectService projectSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Project? p = await db.Projects.FindAsync(id);
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
            for (int i = 0; i < req.Ids.Count; i++)
            {
                int id = req.Ids[i];
                await db.Projects.Where(x => x.Id == id).ExecuteUpdateAsync(s => s.SetProperty(x => x.Order, i + 1));
            }
            cvCache.InvalidateAndRegenerate();
            projectSvc.InvalidateCache();
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
