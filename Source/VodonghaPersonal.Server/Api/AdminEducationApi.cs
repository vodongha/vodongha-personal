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
            item.Id = Guid.NewGuid();
            db.Educations.Add(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            eduSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapPut("/{id:guid}", async (Guid id, Education item, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, EducationService eduSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = id;
            db.Educations.Update(item);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            eduSvc.InvalidateCache();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{id:guid}", async (Guid id, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, EducationService eduSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Education? e = await db.Educations.FindAsync(id);
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
