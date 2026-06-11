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

        group.MapGet("/sessions/{id:guid}", async (Guid id, ChatService svc) =>
        {
            ChatSession? session = await svc.GetSessionAsync(id);
            return session is null ? Results.NotFound() : Results.Ok(session);
        });

        group.MapGet("/sessions/{id:guid}/messages", async (Guid id, ChatService svc) =>
        {
            List<ChatMessage> messages = await svc.GetMessagesAsync(id);
            return Results.Ok(messages);
        });

        group.MapPost("/sessions/{id:guid}/message", async (Guid id, ChatMessageRequest req, ChatService svc) =>
        {
            ChatMessage message = await svc.SendUserMessageAsync(id, req.Content);
            return Results.Ok(message);
        }).DisableAntiforgery();
    }
}
