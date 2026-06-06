using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace vodongha.Services;

public class TelegramService
{
    private readonly HttpClient _http;
    private readonly string _token;
    private readonly long _chatId;

    public TelegramService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _token = config["Telegram:BotToken"] ?? "";
        _chatId = long.TryParse(config["Telegram:ChatId"], out long id) ? id : 0;
    }

    private string BaseUrl => $"https://api.telegram.org/bot{_token}";

    public async Task<long?> CreateTopicAsync(string title)
    {
        if (_chatId == 0 || string.IsNullOrEmpty(_token))
        {
            return null;
        }

        HttpResponseMessage response = await _http.PostAsJsonAsync(
            $"{BaseUrl}/createForumTopic",
            new { chat_id = _chatId, name = title }
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

    public async Task<long?> SendMessageAsync(string text, long threadId)
    {
        if (_chatId == 0 || string.IsNullOrEmpty(_token))
        {
            return null;
        }

        HttpResponseMessage response = await _http.PostAsJsonAsync(
            $"{BaseUrl}/sendMessage",
            new { chat_id = _chatId, message_thread_id = threadId, text = text }
        );

        string json = await response.Content.ReadAsStringAsync();
        JsonDocument doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("result", out JsonElement result) &&
            result.TryGetProperty("message_id", out JsonElement msgId))
        {
            return msgId.GetInt64();
        }

        return null;
    }
}
