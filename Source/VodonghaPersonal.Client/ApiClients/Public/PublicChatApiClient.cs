using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class PublicChatApiClient(HttpClient http)
{
    public async Task<ChatSession?> CreateSessionAsync(string name, string? phone, string? email)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync("/api/chat/sessions", new ChatSessionCreateRequest(name, phone ?? "", email ?? ""));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ChatSession>();
    }

    public async Task<ChatSession?> GetSessionAsync(Guid id)
        => await http.GetFromJsonAsync<ChatSession>($"/api/chat/sessions/{id}");

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid id)
        => await http.GetFromJsonAsync<List<ChatMessage>>($"/api/chat/sessions/{id}/messages") ?? [];

    public async Task<ChatMessage?> SendMessageAsync(Guid sessionId, string content)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync($"/api/chat/sessions/{sessionId}/message", new ChatMessageRequest(content));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ChatMessage>();
    }
}
