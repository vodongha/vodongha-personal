using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace vodongha.Services;

public class TelegramService
{
    private readonly HttpClient _http;
    private readonly AppSecretsService _secrets;

    public TelegramService(HttpClient http, AppSecretsService secrets)
    {
        _http    = http;
        _secrets = secrets;
    }

    private string Token  => _secrets.GetValue("Telegram:BotToken") ?? "";
    private long   ChatId => long.TryParse(_secrets.GetValue("Telegram:ChatId"), out long id) ? id : 0;
    private string BaseUrl => $"https://api.telegram.org/bot{Token}";

    public async Task<long?> CreateTopicAsync(string title)
    {
        if (ChatId == 0 || string.IsNullOrEmpty(Token))
        {
            return null;
        }

        HttpResponseMessage response = await _http.PostAsJsonAsync(
            $"{BaseUrl}/createForumTopic",
            new { chat_id = ChatId, name = title }
        );

        string json = await response.Content.ReadAsStringAsync();
        JsonDocument doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("result", out JsonElement result) &&
            result.TryGetProperty("message_thread_id", out JsonElement threadId))
        {
            return threadId.GetInt64();
        }

        return null;
    }

    public async Task<bool> DeleteTopicAsync(long threadId)
    {
        if (ChatId == 0 || string.IsNullOrEmpty(Token))
        {
            return false;
        }

        try
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync(
                $"{BaseUrl}/deleteForumTopic",
                new { chat_id = ChatId, message_thread_id = threadId }
            );
            string json = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("result", out JsonElement result) && result.GetBoolean();
        }
        catch
        {
            return false;
        }
    }

    public async Task SendTypingAsync(long threadId)
    {
        if (ChatId == 0 || string.IsNullOrEmpty(Token))
        {
            return;
        }

        try
        {
            await _http.PostAsJsonAsync(
                $"{BaseUrl}/sendChatAction",
                new { chat_id = ChatId, message_thread_id = threadId, action = "typing" }
            );
        }
        catch
        {
            // Non-critical — ignore errors
        }
    }

    /// <summary>
    /// Returns (messageId, topicDeleted).
    /// topicDeleted = true means Telegram replied "message thread not found" — the topic no longer exists.
    /// </summary>
    public async Task<(long? MessageId, bool TopicDeleted)> SendMessageAsync(string text, long threadId)
    {
        if (ChatId == 0 || string.IsNullOrEmpty(Token))
        {
            return (null, false);
        }

        HttpResponseMessage response = await _http.PostAsJsonAsync(
            $"{BaseUrl}/sendMessage",
            new { chat_id = ChatId, message_thread_id = threadId, text = text }
        );

        string json = await response.Content.ReadAsStringAsync();
        JsonDocument doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("result", out JsonElement result) &&
            result.TryGetProperty("message_id", out JsonElement msgId))
        {
            return (msgId.GetInt64(), false);
        }

        // Detect deleted/missing topic: {"ok":false,"error_code":400,"description":"Bad Request: message thread not found"}
        bool topicDeleted = false;
        if (doc.RootElement.TryGetProperty("description", out JsonElement desc))
        {
            string description = desc.GetString() ?? "";
            topicDeleted = description.Contains("message thread not found", StringComparison.OrdinalIgnoreCase)
                        || description.Contains("thread not found", StringComparison.OrdinalIgnoreCase);
        }

        return (null, topicDeleted);
    }
}
