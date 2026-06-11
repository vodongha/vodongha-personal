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
            skill.Id = Guid.NewGuid();
            db.Skills.Add(skill);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            skillSvc.InvalidateCache();
            return Results.Ok(skill);
        }).DisableAntiforgery();

        group.MapPut("/{id:guid}", async (Guid id, Skill skill, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, SkillService skillSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            skill.Id = id;
            db.Skills.Update(skill);
            await db.SaveChangesAsync();
            cvCache.InvalidateAndRegenerate();
            skillSvc.InvalidateCache();
            return Results.Ok(skill);
        }).DisableAntiforgery();

        group.MapDelete("/{id:guid}", async (Guid id, IDbContextFactory<AppDbContext> dbFactory, CvCacheService cvCache, SkillService skillSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Skill? skill = await db.Skills.FindAsync(id);
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
