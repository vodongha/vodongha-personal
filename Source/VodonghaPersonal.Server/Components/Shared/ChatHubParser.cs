namespace VodonghaPersonal.Components.Shared;

/// <summary>
/// Parses the anonymous object pushed by SignalR hub methods (ReceiveMessage) into a typed record.
/// Both ChatWidget and AdminChats receive the same hub payload — centralising the parse logic here
/// avoids copy-paste drift if the payload shape ever changes.
/// </summary>
internal static class ChatHubParser
{
    internal record HubMessage(int Id, string Content, bool IsFromUser, DateTime SentAt);

    internal static HubMessage Parse(object msg)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(msg);
        using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
        System.Text.Json.JsonElement root = doc.RootElement;

        int id = root.TryGetProperty("id", out System.Text.Json.JsonElement idEl)
            ? idEl.GetInt32() : 0;
        string content = root.TryGetProperty("content", out System.Text.Json.JsonElement contentEl)
            ? contentEl.GetString() ?? "" : "";
        bool isFromUser = root.TryGetProperty("isFromUser", out System.Text.Json.JsonElement fromEl)
            && fromEl.GetBoolean();
        DateTime sentAt = root.TryGetProperty("sentAt", out System.Text.Json.JsonElement sentEl)
            ? sentEl.GetDateTime() : DateTime.UtcNow;

        return new HubMessage(id, content, isFromUser, sentAt);
    }
}
