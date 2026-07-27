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

        group.MapGet("/paged", async (int? page, int? pageSize, string? search, string? sortBy, string? sortDir, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            IQueryable<Experience> q = db.Experiences;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = $"%{search.Trim()}%";
                q = q.Where(x => EF.Functions.Like(x.Company.ToUpper(), s.ToUpper()) || EF.Functions.Like(x.Role.ToUpper(), s.ToUpper()) || (x.Location != null && EF.Functions.Like(x.Location.ToUpper(), s.ToUpper())));
            }
            bool desc = sortDir == "desc";
            q = sortBy switch
            {
                "Company" => desc ? q.OrderByDescending(x => x.Company) : q.OrderBy(x => x.Company),
                "Role" => desc ? q.OrderByDescending(x => x.Role) : q.OrderBy(x => x.Role),
                "Location" => desc ? q.OrderByDescending(x => x.Location) : q.OrderBy(x => x.Location),
                _ => q.OrderBy(x => x.Order).ThenBy(x => x.Id)
            };
            return Results.Ok(await q.ToPagedResultAsync(page ?? 0, pageSize ?? 10));
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
            db.Entry(existing).CurrentValues.SetValues(item);
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
