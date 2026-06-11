using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminExperienceApi
{
    public static void MapAdminExperienceApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/experience").RequireAuthorization();

        group.MapGet("/", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            List<Experience> items = await db.Experiences.OrderBy(e => e.Order).ToListAsync();
            return Results.Ok(items);
        });

        group.MapPost("/", async (Experience item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ExperienceService expSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = 0;
            if (item.IsCurrent) { item.EndYear = null; item.EndMonth = null; }
            db.Experiences.Add(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            expSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapPut("/{id:int}", async (int id, Experience item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ExperienceService expSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = id;
            if (item.IsCurrent) { item.EndYear = null; item.EndMonth = null; }
            db.Experiences.Update(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            expSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{id:int}", async (int id, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ExperienceService expSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Experience? e = await db.Experiences.FindAsync(id);
            if (e != null)
            {
                db.Experiences.Remove(e);
                await db.SaveChangesAsync();
                cvCache.InvalidateAndRegenerate();
                expSvc.InvalidateCache();
            }
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
