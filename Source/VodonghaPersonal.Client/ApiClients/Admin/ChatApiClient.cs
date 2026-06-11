using System.Net.Http.Json;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.ApiClients;

public class ChatApiClient(HttpClient http)
{
    public async Task<List<ChatSession>> GetSessionsAsync()
        => await http.GetFromJsonAsync<List<ChatSession>>("/api/admin/chat/sessions") ?? [];

    public async Task<ChatSession?> GetSessionAsync(Guid id)
        => await http.GetFromJsonAsync<ChatSession>($"/api/admin/chat/sessions/{id}");

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid sessionId)
        => await http.GetFromJsonAsync<List<ChatMessage>>($"/api/admin/chat/sessions/{sessionId}/messages") ?? [];

    public async Task<ChatMessage?> SendReplyAsync(Guid sessionId, string content)
    {
        HttpResponseMessage resp = await http.PostAsJsonAsync($"/api/admin/chat/sessions/{sessionId}/reply", new ChatReplyRequest(content));
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<ChatMessage>();
    }

    public async Task MarkReadAsync(Guid sessionId)
    {
        HttpResponseMessage resp = await http.PutAsJsonAsync($"/api/admin/chat/sessions/{sessionId}/read", new { });
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        HttpResponseMessage resp = await http.DeleteAsync($"/api/admin/chat/sessions/{sessionId}");
        resp.EnsureSuccessStatusCode();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        int count = await http.GetFromJsonAsync<int>("/api/admin/chat/unread-count");
        return count;
    }
}
