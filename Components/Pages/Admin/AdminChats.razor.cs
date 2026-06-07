using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminChats : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private List<ChatSession> _sessions = [];
    private List<ChatMessage> _messages = [];
    private int? _selectedSessionId;
    private ChatSession? _selectedSession;
    private string _replyText = "";
    private bool _sending;
    private bool _otherIsTyping;
    private int _unreadChatCount;
    private HubConnection? _hubConnection;
    private CancellationTokenSource? _typingCts;

    protected override async Task OnInitializedAsync()
    {
        _sessions = await ChatSvc.GetSessionsAsync();
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            await ConnectHubAsync();
        }
        catch
        {
            // Hub not available during SSR — ignore
        }
    }

    private async Task ConnectHubAsync()
    {
        if (_hubConnection != null)
        {
            return;
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(Nav.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<object>("ReceiveMessage", async msg =>
        {
            _otherIsTyping = false;

            if (!_selectedSessionId.HasValue)
            {
                return;
            }

            string json = System.Text.Json.JsonSerializer.Serialize(msg);
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
            System.Text.Json.JsonElement root = doc.RootElement;

            int id = root.TryGetProperty("id", out System.Text.Json.JsonElement idEl) ? idEl.GetInt32() : 0;
            string content = root.TryGetProperty("content", out System.Text.Json.JsonElement contentEl) ? contentEl.GetString() ?? "" : "";
            bool isFromUser = root.TryGetProperty("isFromUser", out System.Text.Json.JsonElement fromEl) && fromEl.GetBoolean();
            DateTime sentAt = root.TryGetProperty("sentAt", out System.Text.Json.JsonElement sentEl) ? sentEl.GetDateTime() : DateTime.UtcNow;

            _messages.Add(new ChatMessage { Id = id, Content = content, IsFromUser = isFromUser, SentAt = sentAt });
            _sessions = await ChatSvc.GetSessionsAsync();
            _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "adminChatMessages");
        });

        _hubConnection.On("TypingStarted", async () =>
        {
            _otherIsTyping = true;
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "adminChatMessages");
        });

        _hubConnection.On("TypingStopped", async () =>
        {
            _otherIsTyping = false;
            await InvokeAsync(StateHasChanged);
        });

        // Refresh session list when any session gets a new message
        _hubConnection.On<int>("SessionUpdated", async updatedSessionId =>
        {
            _sessions = await ChatSvc.GetSessionsAsync();
            _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
            await InvokeAsync(StateHasChanged);
        });

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinAdminGroup");
    }

    private async Task SelectSession(int sessionId)
    {
        // Leave old session group
        if (_selectedSessionId.HasValue && _hubConnection != null)
        {
            await _hubConnection.InvokeAsync("LeaveSession", _selectedSessionId.Value.ToString());
        }

        _selectedSessionId = sessionId;
        _selectedSession = _sessions.FirstOrDefault(s => s.Id == sessionId);
        _messages = await ChatSvc.GetMessagesAsync(sessionId);
        _replyText = "";
        _otherIsTyping = false;

        // Mark session as read
        await ChatSvc.MarkSessionReadAsync(sessionId);
        if (_selectedSession != null)
        {
            _selectedSession.HasUnread = false;
        }
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();

        // Join new session group
        if (_hubConnection != null)
        {
            await _hubConnection.InvokeAsync("JoinSession", sessionId.ToString());
        }

        await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "adminChatMessages");
    }

    private async Task CloseChat()
    {
        if (_selectedSessionId.HasValue && _hubConnection != null)
        {
            await _hubConnection.InvokeAsync("LeaveSession", _selectedSessionId.Value.ToString());
        }

        _selectedSessionId = null;
        _selectedSession = null;
        _messages = [];
        _otherIsTyping = false;
    }

    private async Task SendReply()
    {
        if (string.IsNullOrWhiteSpace(_replyText) || _sending || !_selectedSessionId.HasValue)
        {
            return;
        }

        await StopTypingAsync();

        string content = _replyText.Trim();
        _replyText = "";
        _sending = true;

        try
        {
            ChatMessage msg = await ChatSvc.SendAdminReplyAsync(_selectedSessionId.Value, content);
            _messages.Add(msg);
            _sessions = await ChatSvc.GetSessionsAsync();
            await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "adminChatMessages");
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task OnReplyKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendReply();
        }
    }

    private async Task OnReplyInput(ChangeEventArgs e)
    {
        _replyText = e.Value?.ToString() ?? "";

        if (_hubConnection == null || !_selectedSessionId.HasValue)
        {
            return;
        }

        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();

        await _hubConnection.InvokeAsync("StartTyping", _selectedSessionId.Value.ToString());

        try
        {
            await Task.Delay(2000, _typingCts.Token);
            await StopTypingAsync();
        }
        catch (OperationCanceledException)
        {
            // Cancelled by next keystroke — fine
        }
    }

    private async Task StopTypingAsync()
    {
        _typingCts?.Cancel();
        if (_hubConnection != null && _selectedSessionId.HasValue)
        {
            await _hubConnection.InvokeAsync("StopTyping", _selectedSessionId.Value.ToString());
        }
    }

    public async ValueTask DisposeAsync()
    {
        _typingCts?.Cancel();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
