using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Api;

public static class PublicChatApi
{
    public static void MapPublicChatApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/chat");

        group.MapPost("/sessions", async (ChatSessionCreateRequest req, ChatService svc) =>
        {
            ChatSession session = await svc.CreateSessionAsync(req.Name, req.Phone, req.Email);
            return Results.Ok(session);
        }).DisableAntiforgery();

        group.MapGet("/sessions/{rid:guid}", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        group.MapGet("/sessions/{rid:guid}/messages", async (Guid rid, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            List<ChatMessage> messages = await svc.GetMessagesAsync(session.Id);
            return Results.Ok(messages);
        });

        group.MapPost("/sessions/{rid:guid}/message", async (Guid rid, ChatMessageRequest req, IDbContextFactory<AppDbContext> dbFactory, ChatService svc) =>
        {
            await using AppDbContext db = await dbFactory.CreateDbContextAsync();
            ChatSession? session = await db.ChatSessions.FirstOrDefaultAsync(s => s.Rid == rid);
            if (session is null) { return Results.NotFound(); }
            ChatMessage message = await svc.SendUserMessageAsync(session.Id, req.Content);
            return Results.Ok(message);
        }).DisableAntiforgery();
    }
}
