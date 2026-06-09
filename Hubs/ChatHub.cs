using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using vodongha.Services;

namespace vodongha.Hubs;

public class ChatHub : Hub
{
    private readonly ChatService _chatSvc;

    public ChatHub(ChatService chatSvc)
    {
        _chatSvc = chatSvc;
    }

    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    [Authorize(Roles = "Admin")]
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
    }

    /// <summary>
    /// Called by the reader to notify the sender their messages have been read.
    /// lastReadId = the highest message ID the caller has now seen.
    /// </summary>
    public async Task MarkRead(string sessionId, int lastReadId)
    {
        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("MessagesRead", lastReadId);
    }

    public async Task StartTyping(string sessionId)
    {
        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("TypingStarted");

        // Forward typing indicator to Telegram so admin sees "typing..." in the topic
        if (int.TryParse(sessionId, out int id))
        {
            await _chatSvc.SendTypingToTelegramAsync(id);
        }
    }

    public async Task StopTyping(string sessionId)
    {
        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("TypingStopped");
    }
}
