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

    private bool CanStartChat => !string.IsNullOrWhiteSpace(_name) && !string.IsNullOrWhiteSpace(_email);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        try
        {
            ProtectedBrowserStorageResult<int> result = await LocalStorage.GetAsync<int>("chatSessionId");
            if (result.Success && result.Value > 0)
            {
                _sessionId = result.Value;
                ChatSession? session = await ChatSvc.GetSessionAsync(_sessionId.Value);
                if (session != null)
                {
                    _state = ChatState.Chat;
                    _messages = (await ChatSvc.GetMessagesAsync(_sessionId.Value))
                        .Select(m => new ChatMessageDto(m.Id, m.Content, m.IsFromUser, m.SentAt))
                        .ToList();
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

    private void ToggleOpen()
    {
        if (_state == ChatState.Closed)
        {
            _state = _sessionId.HasValue ? ChatState.Chat : ChatState.Form;
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

        // Stop typing indicator before sending
        await StopTypingAsync();

        string content = _inputText.Trim();
        _inputText = "";
        _sending = true;

        try
        {
            ChatMessage msg = await ChatSvc.SendUserMessageAsync(_sessionId.Value, content);
            _messages.Add(new ChatMessageDto(msg.Id, msg.Content, msg.IsFromUser, msg.SentAt));
            await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "chatMessages");
        }
        finally
        {
            _sending = false;
        }
    }

    private async Task OnTypingInput(Microsoft.AspNetCore.Components.ChangeEventArgs e)
    {
        _inputText = e.Value?.ToString() ?? "";

        if (_hubConnection == null || !_sessionId.HasValue)
        {
            return;
        }

        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();

        await _hubConnection.InvokeAsync("StartTyping", _sessionId.Value.ToString());

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
        if (_hubConnection != null && _sessionId.HasValue)
        {
            await _hubConnection.InvokeAsync("StopTyping", _sessionId.Value.ToString());
        }
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
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("chatUtils.scrollToBottom", "chatMessages");
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
