using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class AdminContactsApi
{
    public static void MapAdminContactsApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/contacts").RequireAuthorization();

        group.MapGet("/", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            List<ContactMessage> messages = await db.ContactMessages.OrderByDescending(m => m.SentAt).ToListAsync();
            return Results.Ok(messages);
        });

        group.MapPut("/{rid:guid}/read", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ContactMessage? entity = await db.ContactMessages.FirstOrDefaultAsync(m => m.Rid == rid);
            if (entity != null)
            {
                entity.IsRead = true;
                await db.SaveChangesAsync();
            }
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapPut("/read-all", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            await db.ContactMessages.Where(m => !m.IsRead).ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapDelete("/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            await db.ContactMessages.Where(m => m.Rid == rid).ExecuteDeleteAsync();
            return Results.Ok();
        }).DisableAntiforgery();
    }
}
