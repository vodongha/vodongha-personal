using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

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

        group.MapGet("/sessions/{id:int}", async (int id, ChatService svc) =>
        {
            ChatSession? session = await svc.GetSessionAsync(id);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        group.MapGet("/sessions/{id:int}/messages", async (int id, ChatService svc) =>
        {
            List<ChatMessage> messages = await svc.GetMessagesAsync(id);
            return Results.Ok(messages);
        });

        group.MapPost("/sessions/{id:int}/reply", async (int id, ChatReplyRequest req, ChatService svc) =>
        {
            ChatMessage msg = await svc.SendAdminReplyAsync(id, req.Content);
            return Results.Ok(msg);
        }).DisableAntiforgery();

        group.MapPut("/sessions/{id:int}/read", async (int id, ChatService svc) =>
        {
            await svc.MarkSessionReadAsync(id);
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapDelete("/sessions/{id:int}", async (int id, ChatService svc) =>
        {
            await svc.DeleteSessionAsync(id);
            return Results.Ok();
        }).DisableAntiforgery();

        group.MapGet("/unread-count", async (ChatService svc) =>
        {
            int count = await svc.GetUnreadCountAsync();
            return Results.Ok(count);
        });
    }
}
