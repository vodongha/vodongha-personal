using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class ChatWidget : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private ProtectedLocalStorage LocalStorage { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private enum ChatState { Closed, Form, Chat }

    private ChatState _state = ChatState.Closed;
    private string _name = "";
    private string _phone = "";
    private string _email = "";
    private string _inputText = "";
    private bool _loading;
    private bool _sending;
    private bool _otherIsTyping;
    private int? _sessionId;
    private List<ChatMessageDto> _messages = [];
    private HubConnection? _hubConnection;
    private CancellationTokenSource? _typingCts;

    // Unread tracking
    private int _unreadCount;
    private int _lastReadMessageId;    // last admin message ID the user has seen
    private int _adminReadUpToId;      // last user message ID the admin has read (for ✓✓ on user's outgoing)
    private int _unreadDividerIndex = -1;  // index in _messages where the "new messages" divider is shown

    private bool _pendingScrollToUnread;
    private int _inputKey;  // increment on send to force textarea DOM recreation

    private bool CanStartChat => !string.IsNullOrWhiteSpace(_name) && !string.IsNullOrWhiteSpace(_email);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            if (_pendingScrollToUnread)
            {
                _pendingScrollToUnread = false;
                await JS.InvokeVoidAsync("chatUtils.scrollToUnread", "chatMessages");
            }
            return;
        }

        try
        {
            ProtectedBrowserStorageResult<int> sessionResult = await LocalStorage.GetAsync<int>("chatSessionId");
            if (sessionResult.Success && sessionResult.Value > 0)
            {
                _sessionId = sessionResult.Value;
                ChatSession? session = await ChatSvc.GetSessionAsync(_sessionId.Value);
                if (session != null)
                {
                    _messages = (await ChatSvc.GetMessagesAsync(_sessionId.Value))
                        .Select(m => new ChatMessageDto(m.Id, m.Content, m.IsFromUser, m.SentAt))
                        .ToList();

                    // Restore last-read pointer and compute initial unread count
                    ProtectedBrowserStorageResult<int> lastReadResult = await LocalStorage.GetAsync<int>("chatLastReadId");
                    if (lastReadResult.Success && lastReadResult.Value > 0)
                    {
                        _lastReadMessageId = lastReadResult.Value;
                        _unreadCount = _messages.Count(m => !m.IsFromUser && m.Id > _lastReadMessageId);

                        if (_unreadCount > 0)
                        {
                            // Keep closed so badge shows; divider will be set when user opens
                            _state = ChatState.Closed;
                        }
                        else
                        {
                            _state = ChatState.Chat;
                        }
                    }
                    else
                    {
                        // First visit or no saved pointer — mark all current messages as read
                        _lastReadMessageId = _messages.Count > 0 ? _messages.Max(m => m.Id) : 0;
                        _unreadCount = 0;
                        _ = SaveLastReadAsync();
                        _state = ChatState.Chat;
                    }

                    // Restore admin-read pointer (for ✓✓ on user's outgoing messages)
                    ProtectedBrowserStorageResult<int> adminReadResult = await LocalStorage.GetAsync<int>("chatAdminReadId");
                    if (adminReadResult.Success && adminReadResult.Value > 0)
                    {
                        _adminReadUpToId = adminReadResult.Value;
                    }

                    await ConnectHubAsync();
                    StateHasChanged();
                }
            }
        }
        catch
        {
            // localStorage not available (SSR), ignore
        }
    }

    private async Task ToggleOpen()
    {
        if (_state == ChatState.Closed)
        {
            _state = _sessionId.HasValue ? ChatState.Chat : ChatState.Form;
            if (_state == ChatState.Chat)
            {
                SetUnreadDivider();
                await MarkAllReadAsync();
                // If there are unread messages, scroll to the divider; otherwise scroll to bottom
                _pendingScrollToUnread = true;
            }
        }
        else
        {
            _state = ChatState.Closed;
        }
    }

    private void Close()
    {
        _state = ChatState.Closed;
    }

    // Compute and freeze the divider position BEFORE resetting the unread count
    private void SetUnreadDivider()
    {
        _unreadDividerIndex = _messages.FindIndex(m => !m.IsFromUser && m.Id > _lastReadMessageId);
    }

    private async Task MarkAllReadAsync()
    {
        if (_messages.Count == 0)
        {
            return;
        }

        _lastReadMessageId = _messages.Max(m => m.Id);
        _unreadCount = 0;
        await SaveLastReadAsync();

        // Notify the other party (admin) that user has read their messages
        if (_hubConnection != null && _sessionId.HasValue)
        {
            int lastAdminMsgId = _messages.Where(m => !m.IsFromUser).Select(m => (int?)m.Id).Max() ?? 0;
            if (lastAdminMsgId > 0)
            {
                await _hubConnection.InvokeAsync("MarkRead", _sessionId.Value.ToString(), lastAdminMsgId);
            }
        }
    }

    private async Task SaveLastReadAsync()
    {
        try
        {
            await LocalStorage.SetAsync("chatLastReadId", _lastReadMessageId);
        }
        catch
        {
            // ignore
        }
    }

    private async Task StartChat()
    {
        if (!CanStartChat || _loading)
        {
            return;
        }

        _loading = true;
        try
        {
            ChatSession session = await ChatSvc.CreateSessionAsync(_name.Trim(), _phone.Trim(), _email.Trim());
            _sessionId = session.Id;
            await LocalStorage.SetAsync("chatSessionId", session.Id);
            _state = ChatState.Chat;
            _messages = [];
            _unreadCount = 0;
            await ConnectHubAsync();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_inputText) || _sending || !_sessionId.HasValue)
        {
            return;
        }

        string content = _inputText.Trim();
        _inputText = "";
        _inputKey++;        // Force textarea DOM recreation so it clears reliably
        _sending = true;

        // Stop typing indicator — fire-and-forget, no need to await
        _ = StopTypingAsync();

        // Optimistic: show message instantly before the API responds
        ChatMessageDto optimistic = new(0, content, true, DateTime.UtcNow);
        _messages.Add(optimistic);
        StateHasChanged();
        await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "chatMessages");

        try
        {
            ChatMessage msg = await ChatSvc.SendUserMessageAsync(_sessionId.Value, content);
            // Replace optimistic placeholder with real message (gets its DB Id for read receipts)
            int idx = _messages.FindIndex(m => m.Id == 0 && m.Content == content && m.IsFromUser);
            if (idx >= 0)
            {
                _messages[idx] = new ChatMessageDto(msg.Id, msg.Content, msg.IsFromUser, msg.SentAt);
            }
        }
        catch
        {
            // Revert on failure
            _messages.Remove(optimistic);
            _inputText = content;
        }
        finally
        {
            _sending = false;
        }
    }

    private void OnTypingInput(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        _inputText = e.Value?.ToString() ?? "";

        if (_hubConnection == null || !_sessionId.HasValue)
        {
            return;
        }

        // Cancel previous debounce timer
        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();

        // Fire-and-forget — never await SignalR inside an oninput handler (blocks keypress queue)
        _ = SendTypingSignalsAsync(_typingCts.Token);
    }

    private async Task SendTypingSignalsAsync(CancellationToken ct)
    {
        if (_hubConnection == null || !_sessionId.HasValue) return;
        try
        {
            _ = _hubConnection.InvokeAsync("StartTyping", _sessionId.Value.ToString(), ct);
            await Task.Delay(2000, ct);
            _ = _hubConnection.InvokeAsync("StopTyping", _sessionId.Value.ToString());
        }
        catch (OperationCanceledException)
        {
            // Cancelled by next keystroke — fine
        }
    }

    private Task StopTypingAsync()
    {
        _typingCts?.Cancel();
        if (_hubConnection != null && _sessionId.HasValue)
        {
            // Fire-and-forget — caller does not need to await
            _ = _hubConnection.InvokeAsync("StopTyping", _sessionId.Value.ToString());
        }
        return Task.CompletedTask;
    }

    private async Task OnKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private async Task ConnectHubAsync()
    {
        if (_hubConnection != null || !_sessionId.HasValue)
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

            string json = System.Text.Json.JsonSerializer.Serialize(msg);
            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
            System.Text.Json.JsonElement root = doc.RootElement;

            int id = root.TryGetProperty("id", out System.Text.Json.JsonElement idEl) ? idEl.GetInt32() : 0;
            string content = root.TryGetProperty("content", out System.Text.Json.JsonElement contentEl) ? contentEl.GetString() ?? "" : "";
            bool isFromUser = root.TryGetProperty("isFromUser", out System.Text.Json.JsonElement fromEl) && fromEl.GetBoolean();
            DateTime sentAt = root.TryGetProperty("sentAt", out System.Text.Json.JsonElement sentEl) ? sentEl.GetDateTime() : DateTime.UtcNow;

            _messages.Add(new ChatMessageDto(id, content, isFromUser, sentAt));

            if (!isFromUser)
            {
                if (_state == ChatState.Closed)
                {
                    // Chat is closed — increment badge
                    _unreadCount++;
                }
                else
                {
                    // Chat is open — user sees it immediately; clear any lingering divider
                    _unreadDividerIndex = -1;
                    _lastReadMessageId = id;
                    _ = SaveLastReadAsync();
                    // Notify admin that user has read this message
                    if (_hubConnection != null && _sessionId.HasValue)
                    {
                        _ = _hubConnection.InvokeAsync("MarkRead", _sessionId.Value.ToString(), id);
                    }
                }
            }

            await InvokeAsync(StateHasChanged);
            if (_state == ChatState.Chat)
            {
                await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "chatMessages");
            }
        });

        // Admin read receipt — update ✓ → ✓✓ on user's outgoing messages
        _hubConnection.On<int>("AdminRead", async lastReadId =>
        {
            _adminReadUpToId = Math.Max(_adminReadUpToId, lastReadId);
            try { await LocalStorage.SetAsync("chatAdminReadId", _adminReadUpToId); } catch { }
            await InvokeAsync(StateHasChanged);
        });

        _hubConnection.On("TypingStarted", async () =>
        {
            _otherIsTyping = true;
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "chatMessages");
        });

        _hubConnection.On("TypingStopped", async () =>
        {
            _otherIsTyping = false;
            await InvokeAsync(StateHasChanged);
        });

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinSession", _sessionId.Value.ToString());
    }

    public async ValueTask DisposeAsync()
    {
        _typingCts?.Cancel();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private record ChatMessageDto(int Id, string Content, bool IsFromUser, DateTime SentAt);
}
