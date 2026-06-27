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

        group.MapGet("/paged", async (int? page, int? pageSize, string? search, string? sortBy, string? sortDir, IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            IQueryable<ContactMessage> q = db.ContactMessages;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = $"%{search.Trim()}%";
                q = q.Where(x => EF.Functions.ILike(x.Name, s) || EF.Functions.ILike(x.Email, s) || EF.Functions.ILike(x.Subject, s));
            }
            bool desc = sortDir == "desc";
            q = sortBy switch
            {
                "Subject" => desc ? q.OrderByDescending(x => x.Subject) : q.OrderBy(x => x.Subject),
                "SentAt" => desc ? q.OrderByDescending(x => x.SentAt) : q.OrderBy(x => x.SentAt),
                _ => q.OrderByDescending(x => x.SentAt).ThenByDescending(x => x.Id)
            };
            return Results.Ok(await q.ToPagedResultAsync(page ?? 0, pageSize ?? 10));
        });

        group.MapGet("/unread-count", async (IDbContextFactory<AppDbContext> dbFactory) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            int count = await db.ContactMessages.CountAsync(m => !m.IsRead);
            return Results.Ok(count);
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
