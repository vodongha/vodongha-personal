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
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    private bool _loading = true;
    private List<ChatSession> _sessions = [];
    private List<ChatMessage> _messages = [];
    private int? _selectedSessionId;
    private ChatSession? _selectedSession;
    private string _replyText = "";
    private bool _sending;
    private bool _otherIsTyping;
    private int _unreadChatCount;
    private int _deleteSessionId;
    private bool _confirmShow;
    private int _sessionLastReadId;   // user-message ID boundary: messages with Id > this are "new to admin"
    private int _userReadUpToId;      // max admin-message ID the user has read (for ✓✓ on admin's outgoing)
    private Dictionary<int, int> _sessionLastSeenId = new();  // sessionId → lastReadId, persists across re-opens
    private bool _pendingScrollToUnread;
    private HubConnection? _hubConnection;
    private CancellationTokenSource? _typingCts;

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        Tz.OnTimezoneSet += OnTimezoneUpdated;
        _sessions = await ChatSvc.GetSessionsAsync();
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        _loading = false;
    }

    private void OnTimezoneUpdated() => InvokeAsync(StateHasChanged);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            if (_pendingScrollToUnread)
            {
                _pendingScrollToUnread = false;
                await JS.InvokeVoidAsync("chatUtils.scrollToUnread", "adminChatMessages");
            }
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

    private void ConfirmDeleteSession(int sessionId)
    {
        _deleteSessionId = sessionId;
        _confirmShow = true;
    }

    private async Task ExecuteDeleteSession()
    {
        _confirmShow = false;
        await DeleteSessionAsync(_deleteSessionId);
    }

    private async Task DeleteSessionAsync(int sessionId)
    {
        // If the deleted session is currently open, close it first
        if (_selectedSessionId == sessionId)
        {
            await CloseChat();
        }

        await ChatSvc.DeleteSessionAsync(sessionId);
        _sessions.RemoveAll(s => s.Id == sessionId);
        _sessionLastSeenId.Remove(sessionId);
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        Toast.Show("Đã xoá cuộc trò chuyện");
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

            // Skip admin's own replies — already shown via optimistic update in SendReply.
            // Only process incoming user messages here.
            if (!isFromUser) return;

            _messages.Add(new ChatMessage { Id = id, Content = content, IsFromUser = isFromUser, SentAt = sentAt });
            // Admin is watching this session live — auto-mark as read
            if (isFromUser)
            {
                _sessionLastReadId = id;
                await ChatSvc.MarkSessionReadAsync(_selectedSessionId.Value);
            }
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

        // User read receipt — update ✓ → ✓✓ on admin's outgoing messages
        _hubConnection.On<int>("MessagesRead", async lastReadId =>
        {
            _userReadUpToId = Math.Max(_userReadUpToId, lastReadId);
            await InvokeAsync(StateHasChanged);
        });

        // Session deleted by another admin tab — remove it from the list
        _hubConnection.On<int>("SessionDeleted", async deletedSessionId =>
        {
            if (_selectedSessionId == deletedSessionId)
            {
                _selectedSessionId = null;
                _selectedSession = null;
                _messages = [];
                _sessionLastReadId = 0;
                _userReadUpToId = 0;
            }
            _sessions.RemoveAll(s => s.Id == deletedSessionId);
            _sessionLastSeenId.Remove(deletedSessionId);
            _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
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
        // Save last-seen pointer for the session being left
        if (_selectedSessionId.HasValue && _messages.Count > 0)
        {
            _sessionLastSeenId[_selectedSessionId.Value] = _messages.Max(m => m.Id);
        }

        // Leave old session group
        if (_selectedSessionId.HasValue && _hubConnection != null)
        {
            await _hubConnection.InvokeAsync("LeaveSession", _selectedSessionId.Value.ToString());
        }

        _selectedSessionId = sessionId;
        _selectedSession = _sessions.FirstOrDefault(s => s.Id == sessionId);
        bool sessionHasUnread = _selectedSession?.HasUnread ?? false;
        _messages = await ChatSvc.GetMessagesAsync(sessionId);
        _replyText = "";
        _otherIsTyping = false;
        _userReadUpToId = 0;

        // Determine the divider boundary
        if (_sessionLastSeenId.TryGetValue(sessionId, out int storedLastSeen))
        {
            // Re-opening a session: new messages = those after last time admin viewed it
            _sessionLastReadId = storedLastSeen;
        }
        else if (sessionHasUnread)
        {
            // First open with unread: new messages = user messages after admin's last reply
            // Find the last admin message index; user messages after it are "new"
            int lastAdminIdx = -1;
            for (int i = _messages.Count - 1; i >= 0; i--)
            {
                if (!_messages[i].IsFromUser) { lastAdminIdx = i; break; }
            }

            if (lastAdminIdx < 0)
            {
                // Admin has never replied — all user messages are new
                _sessionLastReadId = 0;
            }
            else
            {
                // User messages before lastAdminIdx are "seen"; after = new
                _sessionLastReadId = _messages.Take(lastAdminIdx)
                    .Where(m => m.IsFromUser)
                    .Select(m => (int?)m.Id)
                    .Max() ?? 0;
            }
        }
        else
        {
            // No unread — mark everything as read, no divider
            _sessionLastReadId = _messages.Where(m => m.IsFromUser).Select(m => (int?)m.Id).Max() ?? 0;
        }

        // Mark session as read in DB and notify widget
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

        // Scroll to unread divider if there are new messages, otherwise scroll to bottom
        _pendingScrollToUnread = true;
        StateHasChanged();
    }

    private async Task CloseChat()
    {
        // Save last-seen pointer so re-opens show a divider for new messages
        if (_selectedSessionId.HasValue && _messages.Count > 0)
        {
            _sessionLastSeenId[_selectedSessionId.Value] = _messages.Max(m => m.Id);
        }

        if (_selectedSessionId.HasValue && _hubConnection != null)
        {
            await _hubConnection.InvokeAsync("LeaveSession", _selectedSessionId.Value.ToString());
        }

        _selectedSessionId = null;
        _selectedSession = null;
        _messages = [];
        _otherIsTyping = false;
        _sessionLastReadId = 0;
        _userReadUpToId = 0;
    }

    private async Task SendReply()
    {
        if (string.IsNullOrWhiteSpace(_replyText) || _sending || !_selectedSessionId.HasValue)
        {
            return;
        }

        string content = _replyText.Trim();
        _replyText = "";    // Clear input immediately — @bind:event="oninput" ensures Blazor tracks this
        _sending = true;
        _ = StopTypingAsync(); // fire-and-forget, don't block optimistic display

        // Optimistic: show message instantly
        ChatMessage optimistic = new() { Id = 0, Content = content, IsFromUser = false, SentAt = DateTime.UtcNow, ChatSessionId = _selectedSessionId.Value };
        _messages.Add(optimistic);
        StateHasChanged();
        await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "adminChatMessages");

        try
        {
            ChatMessage msg = await ChatSvc.SendAdminReplyAsync(_selectedSessionId.Value, content);
            // Replace optimistic placeholder with real message
            int idx = _messages.FindIndex(m => m.Id == 0 && m.Content == content && !m.IsFromUser);
            if (idx >= 0)
            {
                _messages[idx] = msg;
            }
            // Admin just sent — they've seen all messages; reset unread pointer
            _sessionLastReadId = _messages.Where(m => m.IsFromUser).Select(m => (int?)m.Id).Max() ?? _sessionLastReadId;
            _sessions = await ChatSvc.GetSessionsAsync();
        }
        catch
        {
            // Revert on failure
            _messages.Remove(optimistic);
            _replyText = content;
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

    // Called by @bind:after — _replyText already updated, no ChangeEventArgs needed
    private void OnReplyInput()
    {
        if (_hubConnection == null || !_selectedSessionId.HasValue) return;
        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();
        _ = SendTypingSignalsAsync(_typingCts.Token);
    }

    private async Task SendTypingSignalsAsync(CancellationToken ct)
    {
        if (_hubConnection == null || !_selectedSessionId.HasValue) return;
        try
        {
            _ = _hubConnection.InvokeAsync("StartTyping", _selectedSessionId.Value.ToString(), ct);
            await Task.Delay(2000, ct);
            _ = _hubConnection.InvokeAsync("StopTyping", _selectedSessionId.Value.ToString());
        }
        catch (OperationCanceledException) { }
    }

    private Task StopTypingAsync()
    {
        _typingCts?.Cancel();
        if (_hubConnection != null && _selectedSessionId.HasValue)
        {
            _ = _hubConnection.InvokeAsync("StopTyping", _selectedSessionId.Value.ToString());
        }
        return Task.CompletedTask;
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public async ValueTask DisposeAsync()
    {
        Loc.OnChanged -= OnLangChanged;
        Tz.OnTimezoneSet -= OnTimezoneUpdated;
        _typingCts?.Cancel();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
