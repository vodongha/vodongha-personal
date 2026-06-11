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

    public async Task<ChatSession?> GetSessionAsync(Guid rid)
        => await http.GetFromJsonAsync<ChatSession>($"/api/chat/sessions/{rid}");

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid rid)
        => await http.GetFromJsonAsync<List<ChatMessage>>($"/api/chat/sessions/{rid}/messages") ?? [];

    public async Task<ChatMessage?> SendMessageAsync(Guid rid, string content)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync($"/api/chat/sessions/{rid}/message", new ChatMessageRequest(content));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ChatMessage>();
    }
}
