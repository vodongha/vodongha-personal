using Microsoft.AspNetCore.SignalR;

namespace vodongha.Hubs;

public class ChatHub : Hub
{
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
    }

    public async Task StartTyping(string sessionId)
    {
        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("TypingStarted");
    }

    public async Task StopTyping(string sessionId)
    {
        await Clients.OthersInGroup($"session_{sessionId}").SendAsync("TypingStopped");
    }
}
