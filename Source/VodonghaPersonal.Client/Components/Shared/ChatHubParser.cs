namespace VodonghaPersonal.Client.Components.Shared;

internal static class ChatHubParser
{
    internal record HubMessage(Guid Id, string Content, bool IsFromUser, DateTime SentAt);

    internal static HubMessage Parse(object msg)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(msg);
        using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
        System.Text.Json.JsonElement root = doc.RootElement;

        Guid id = root.TryGetProperty("id", out System.Text.Json.JsonElement idEl)
            ? idEl.GetGuid() : Guid.Empty;
        string content = root.TryGetProperty("content", out System.Text.Json.JsonElement contentEl)
            ? contentEl.GetString() ?? "" : "";
        bool isFromUser = root.TryGetProperty("isFromUser", out System.Text.Json.JsonElement fromEl)
            && fromEl.GetBoolean();
        DateTime sentAt = root.TryGetProperty("sentAt", out System.Text.Json.JsonElement sentEl)
            ? sentEl.GetDateTime() : DateTime.UtcNow;

        return new HubMessage(id, content, isFromUser, sentAt);
    }
}
