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

        group.MapPut("/{rid:guid}", async (Guid rid, Experience item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ExperienceService expSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Experience? existing = await db.Experiences.FirstOrDefaultAsync(e => e.Rid == rid);
            if (existing is null) { return Results.NotFound(); }
            item.Id = existing.Id;
            item.Rid = rid;
            if (item.IsCurrent) { item.EndYear = null; item.EndMonth = null; }
            db.Experiences.Update(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            expSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, ExperienceService expSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Experience? e = await db.Experiences.FirstOrDefaultAsync(x => x.Rid == rid);
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
