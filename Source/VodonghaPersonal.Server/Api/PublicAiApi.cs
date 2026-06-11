using VodonghaPersonal.Services;

namespace VodonghaPersonal.Api;

public static class PublicAiApi
{
    public static void MapPublicAiApi(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/ai");

        group.MapPost("/ask", async (AiAskRequest req, AiService svc, CancellationToken ct) =>
        {
            List<AiService.AiMessage> messages = req.History
                .Select(m => new AiService.AiMessage(m.Role, m.Content))
                .ToList();

            string? reply = await svc.AskAsync(messages, ct);
            return Results.Ok(reply);
        }).DisableAntiforgery();
    }

    private record AiMessageDto(string Role, string Content);
    private record AiAskRequest(List<AiMessageDto> History);
}
