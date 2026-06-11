using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;

namespace VodonghaPersonal.Api;

public static class AdminChatApi
{
    public static void MapAdminChatApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/admin/chat").RequireAuthorization();

        group.MapGet("/sessions", async (ChatService svc) =>
        {
            List<ChatSession> sessions = await svc.GetSessionsAsync();
            return Results.Ok(sessions);
        });

        group.MapGet("/sessions/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            return Results.Ok(session);
        });

        group.MapGet("/sessions/{rid:guid}/messages", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            List<ChatMessage> messages = await svc.GetMessagesAsync(session.Id);
            return Results.Ok(messages);
        });

        group.MapPost("/sessions/{rid:guid}/reply", async (Guid rid, ChatReplyRequest req, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            ChatMessage msg = await svc.SendAdminReplyAsync(session.Id, req.Content);
            return Results.Ok(msg);
        }).DisableAntiforgery();

        group.MapPut("/sessions/{rid:guid}/read", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            await svc.MarkSessionReadAsync(session.Id);
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapDelete("/sessions/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            await svc.DeleteSessionAsync(session.Id);
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapGet("/unread-count", async (ChatService svc) =>
        {
            int count = await svc.GetUnreadCountAsync();
            return Results.Ok(count);
        });
    }
}
