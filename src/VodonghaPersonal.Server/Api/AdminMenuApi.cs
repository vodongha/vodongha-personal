using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Api;

public static class AdminMenuApi
{
    public static void MapAdminMenuApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/menu-stats", async (
            IDbContextFactory<AppDbContext> dbFactory,
            ChatService chatSvc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            int unreadMessages = await db.ContactMessages.CountAsync(m => !m.IsRead);
            int unreadChats = await chatSvc.GetUnreadCountAsync();
            return Results.Ok(new { unreadMessages, unreadChats });
        }).RequireAuthorization();

        // Save pref key via AppSecrets
        app.MapPost("/api/admin/prefs/{key}", async (string key, AppSecretsService secrets, HttpRequest req) =>
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            await secrets.SaveAsync(key, body);
            return Results.Ok();
        }).RequireAuthorization().DisableAntiforgery();

        app.MapGet("/api/admin/prefs/{key}", async (string key, AppSecretsService secrets) =>
        {
            string? val = await secrets.GetValueAsync(key);
            return Results.Ok(new { value = val ?? "[]" });
        }).RequireAuthorization();
    }
}
