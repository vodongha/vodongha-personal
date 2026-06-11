namespace VodonghaPersonal.Shared.DTOs;

public record AiMessage(string Role, string Content);

public record AiAskRequest(List<AiMessage> History);
