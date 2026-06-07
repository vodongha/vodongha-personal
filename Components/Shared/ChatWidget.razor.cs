using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using PhoneNumbers;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class ChatWidget : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private ProtectedLocalStorage LocalStorage { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    private enum ChatState { Closed, Form, Chat }

    private record Country(string Flag, string RegionCode, string Dial, string Name);

    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    // Priority countries shown at top
    private static readonly string[] PriorityRegions = ["VN", "US", "GB", "AU", "CA", "SG", "JP", "KR", "CN", "FR", "DE"];

    private static readonly List<Country> Countries = BuildCountryList();

    private static List<Country> BuildCountryList()
    {
        var util = PhoneNumberUtil.GetInstance();
        var all = util.GetSupportedRegions()
            .Select(region =>
            {
                int dialCode = util.GetCountryCodeForRegion(region);
                string flag = RegionToFlag(region);
                string name = new System.Globalization.RegionInfo(region).EnglishName;
                return new Country(flag, region, $"+{dialCode}", name);
            })
            .OrderBy(c => c.Name)
            .ToList();

        // Move priority regions to top
        var priority = PriorityRegions
            .Select(r => all.FirstOrDefault(c => c.RegionCode == r))
            .Where(c => c != null)
            .Select(c => c!)
            .ToList();

        var rest = all.Where(c => !PriorityRegions.Contains(c.RegionCode)).ToList();
        return [.. priority, .. rest];
    }

    private static string RegionToFlag(string region)
    {
        if (region.Length != 2) return "🌐";
        return string.Concat(region.Select(c => char.ConvertFromUtf32(c - 'A' + 0x1F1E6)));
    }

    private string _selectedRegion = "VN";
    private Country SelectedCountry => Countries.FirstOrDefault(c => c.RegionCode == _selectedRegion) ?? Countries[0];

    private void OnDialChanged(ChangeEventArgs e)
    {
        _selectedRegion = e.Value?.ToString() ?? "VN";
    }

    private void OnPhoneInput(ChangeEventArgs e)
    {
        string raw = e.Value?.ToString() ?? "";
        if (raw.StartsWith("0")) raw = raw[1..];
        _phone = new string(raw.Where(c => char.IsDigit(c) || c == ' ' || c == '-').ToArray());
    }

    private bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        try
        {
            string digits = new string(phone.Where(char.IsDigit).ToArray());
            string full = $"+{PhoneUtil.GetCountryCodeForRegion(_selectedRegion)}{digits}";
            PhoneNumber parsed = PhoneUtil.Parse(full, _selectedRegion);
            return PhoneUtil.IsValidNumber(parsed);
        }
        catch { return false; }
    }

    private string FullPhone => $"{SelectedCountry.Dial}{new string(_phone.Where(char.IsDigit).ToArray())}";

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

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try { return new System.Net.Mail.MailAddress(email).Address == email.Trim(); }
        catch { return false; }
    }

    private bool CanStartChat =>
        !string.IsNullOrWhiteSpace(_name) &&
        IsValidPhone(_phone) &&
        IsValidEmail(_email);

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

        // Auto-detect country from IP (fire-and-forget, non-blocking)
        _ = Task.Run(async () =>
        {
            try
            {
                string? countryCode = await JS.InvokeAsync<string?>("chatUtils.getCountryCode");
                if (!string.IsNullOrWhiteSpace(countryCode) && Countries.Any(c => c.RegionCode == countryCode))
                {
                    _selectedRegion = countryCode;
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch { }
        });

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
                else
                {
                    // Session was deleted by admin — clear stale localStorage and show form
                    await ClearSessionStorageAsync();
                    _sessionId = null;
                    _state = ChatState.Closed;
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

    private async Task ClearSessionStorageAsync()
    {
        try
        {
            await LocalStorage.DeleteAsync("chatSessionId");
            await LocalStorage.DeleteAsync("chatLastReadId");
            await LocalStorage.DeleteAsync("chatAdminReadId");
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
            ChatSession session = await ChatSvc.CreateSessionAsync(_name.Trim(), FullPhone.Trim(), _email.Trim());
            _sessionId = session.Id;
            await LocalStorage.SetAsync("chatSessionId", session.Id);
            _state = ChatState.Chat;
            _unreadCount = 0;
            await ConnectHubAsync();
            // Load messages from DB so the auto welcome message is visible immediately
            _messages = (await ChatSvc.GetMessagesAsync(session.Id))
                .Select(m => new ChatMessageDto(m.Id, m.Content, m.IsFromUser, m.SentAt))
                .ToList();
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
        _inputText = "";    // @bind:event="oninput" ensures Blazor tracks this correctly
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
        catch (InvalidOperationException ex) when (ex.Message.Contains("Session not found"))
        {
            // Admin deleted the session while user was chatting — reset widget to form
            _messages.Remove(optimistic);
            await ClearSessionStorageAsync();
            _sessionId = null;
            _messages = [];
            _state = ChatState.Form;
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

    // Called by @bind:after — _inputText already updated by @bind, no args needed
    private void OnTypingInput()
    {
        if (_hubConnection == null || !_sessionId.HasValue) return;
        _typingCts?.Cancel();
        _typingCts = new CancellationTokenSource();
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

    protected override void OnInitialized()
    {
        Tz.OnTimezoneSet += OnTimezoneUpdated;
        Lang.OnChange += OnLangChanged;
    }

    private void OnTimezoneUpdated() => InvokeAsync(StateHasChanged);
    private void OnLangChanged() => InvokeAsync(StateHasChanged);

    public async ValueTask DisposeAsync()
    {
        Tz.OnTimezoneSet -= OnTimezoneUpdated;
        Lang.OnChange -= OnLangChanged;
        _typingCts?.Cancel();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private record ChatMessageDto(int Id, string Content, bool IsFromUser, DateTime SentAt);
}
