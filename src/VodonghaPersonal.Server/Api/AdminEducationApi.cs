using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
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

        group.MapPost("/", async (Education item, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = 0;
            db.Educations.Add(item);
            await db.SaveChangesAsync();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapPut("/{id:int}", async (int id, Education item, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            item.Id = id;
            db.Educations.Update(item);
            await db.SaveChangesAsync();
            return Results.Ok(item);
        }).DisableAntiforgery();

        group.MapDelete("/{id:int}", async (int id, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            Education? e = await db.Educations.FindAsync(id);
            if (e != null) { db.Educations.Remove(e); await db.SaveChangesAsync(); }
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
