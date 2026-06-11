using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminEducationApi
{
    public static void MapAdminEducationApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/education").RequireAuthorization();

        group.MapGet("/", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            List<Education> items = await db.Educations.OrderBy(e => e.Order).ToListAsync();
            return Results.Ok(items);
        });

        group.MapPost("/", async (Education item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, EducationService eduSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = 0;
            db.Educations.Add(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            eduSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapPut("/{rid:guid}", async (Guid rid, Education item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, EducationService eduSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Education? existing = await db.Educations.FirstOrDefaultAsync(e => e.Rid == rid);
            if (existing is null) { return Results.NotFound(); }
            item.Id = existing.Id;
            item.Rid = rid;
            db.Educations.Update(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            eduSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, EducationService eduSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Education? e = await db.Educations.FirstOrDefaultAsync(x => x.Rid == rid);
            if (e != null)
            {
                db.Educations.Remove(e);
                await db.SaveChangesAsync();
                cvCache.InvalidateAndRegenerate();
                eduSvc.InvalidateCache();
            }
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
