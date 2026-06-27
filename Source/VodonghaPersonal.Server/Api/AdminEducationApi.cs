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

        group.MapGet("/paged", async (int? page, int? pageSize, string? search, string? sortBy, string? sortDir, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            IQueryable<Education> q = db.Educations;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = $"%{search.Trim()}%";
                q = q.Where(x => EF.Functions.ILike(x.School, s) || EF.Functions.ILike(x.Degree, s) || EF.Functions.ILike(x.Field, s));
            }
            bool desc = sortDir == "desc";
            q = sortBy switch
            {
                "School" => desc ? q.OrderByDescending(x => x.School) : q.OrderBy(x => x.School),
                "Degree" => desc ? q.OrderByDescending(x => x.Degree) : q.OrderBy(x => x.Degree),
                "Field" => desc ? q.OrderByDescending(x => x.Field) : q.OrderBy(x => x.Field),
                _ => q.OrderBy(x => x.Order).ThenBy(x => x.Id)
            };
            return Results.Ok(await q.ToPagedResultAsync(page ?? 0, pageSize ?? 10));
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
            db.Entry(existing).CurrentValues.SetValues(item);
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
