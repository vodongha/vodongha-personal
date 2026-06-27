using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminSkillsApi
{
    public static void MapAdminSkillsApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/skills").RequireAuthorization();

        group.MapGet("/", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            List<Skill> skills = await db.Skills.OrderBy(s => s.Order).ToListAsync();
            return Results.Ok(skills);
        });

        group.MapGet("/paged", async (int? page, int? pageSize, string? search, string? sortBy, string? sortDir, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            IQueryable<Skill> q = db.Skills;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = $"%{search.Trim()}%";
                q = q.Where(x => EF.Functions.ILike(x.Name, s) || EF.Functions.ILike(x.Category, s));
            }
            bool desc = sortDir == "desc";
            q = sortBy switch
            {
                "Name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
                "Proficiency" => desc ? q.OrderByDescending(x => x.Proficiency) : q.OrderBy(x => x.Proficiency),
                "Order" => desc ? q.OrderByDescending(x => x.Order) : q.OrderBy(x => x.Order),
                _ => q.OrderBy(x => x.Order).ThenBy(x => x.Id)
            };
            return Results.Ok(await q.ToPagedResultAsync(page ?? 0, pageSize ?? 10));
        });

        group.MapPost("/", async (Skill skill, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, SkillService skillSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            skill.Id = 0;
            db.Skills.Add(skill);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            skillSvc.InvalidateCache();
            return Results.Ok(skill);
        }).DisableAntiforgery();

        group.MapPut("/{rid:guid}", async (Guid rid, Skill skill, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, SkillService skillSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Skill? existing = await db.Skills.FirstOrDefaultAsync(s => s.Rid == rid);
            if (existing is null) { return Results.NotFound(); }
            skill.Id = existing.Id;
            skill.Rid = rid;
            db.Entry(existing).CurrentValues.SetValues(skill);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            skillSvc.InvalidateCache();
            return Results.Ok(skill);
        }).DisableAntiforgery();

        group.MapDelete("/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, SkillService skillSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Skill? skill = await db.Skills.FirstOrDefaultAsync(s => s.Rid == rid);
            if (skill != null)
            {
                db.Skills.Remove(skill);
                await db.SaveChangesAsync();
                cvCache.InvalidateAndRegenerate();
                skillSvc.InvalidateCache();
            }
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
