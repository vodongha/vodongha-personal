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
            db.Skills.Update(skill);
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
