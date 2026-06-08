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
    [Inject] private PushNotificationService PushSvc { get; set; } = default!;

    private enum ChatState { Closed, Form, Chat }

    private record Country(string Flag, string RegionCode, string Dial, string Name);

    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    // Priority countries shown at top
    private static readonly string[] PriorityRegions = ["VN", "US", "GB", "AU", "CA", "SG", "JP", "KR", "CN", "FR", "DE"];

    private static readonly List<Country> Countries = BuildCountryList();

    private static List<Country> BuildCountryList()
    {
        var util = PhoneNumberUtil.GetInstance();
        var all = new List<Country>();

        foreach (string region in util.GetSupportedRegions())
        {
            try
            {
                int dialCode = util.GetCountryCodeForRegion(region);
                string name = new System.Globalization.RegionInfo(region).EnglishName;
                all.Add(new Country(RegionToFlag(region), region, $"+{dialCode}", name));
            }
            catch
            {
                // Skip unsupported regions (e.g. "001", "AC", "TA")
            }
        }

        all = [.. all.OrderBy(c => c.Name)];

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
    private bool _nameTouched;
    private bool _phoneTouched;
    private bool _emailTouched;
    private Country SelectedCountry => Countries.FirstOrDefault(c => c.RegionCode == _selectedRegion) ?? Countries[0];

    private void OnDialChanged(ChangeEventArgs e)
    {
        _selectedRegion = e.Value?.ToString() ?? _selectedRegion;
    }

    // Called on @onchange (blur) — reads the JS-cleaned value into _phone
    private void OnPhoneInput(ChangeEventArgs e)
    {
        _phone = e.Value?.ToString() ?? "";
    }

    private void OnPhoneBlur()
    {
        _phoneTouched = true;
    }

    private bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        try
        {
            PhoneNumber parsed = PhoneUtil.Parse(phone, _selectedRegion);
            return PhoneUtil.IsValidNumberForRegion(parsed, _selectedRegion);
        }
        catch
        {
            return false;
        }
    }

    private string FullPhone
    {
        get
        {
            string digits = new string(_phone.Where(char.IsDigit).ToArray());
            // Strip leading zero before prepending dial code (e.g. 0929... → +84929...)
            if (digits.StartsWith('0')) digits = digits[1..];
            return $"{SelectedCountry.Dial}{digits}";
        }
    }

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
    private bool _pushDenied;   // true when Notification.permission === 'denied'
    private string _pushHelpUrl = "https://support.google.com/chrome/answer/3220216"; // default Chrome

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

            // Init JS helpers — pass DotNetRef for typing indicator callback
        DotNetObjectReference<ChatWidget> dotNetRef = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("chatDial.init", dotNetRef);
        await JS.InvokeVoidAsync("chatUtils.initInput", dotNetRef);

        // Auto-select country from already-detected timezone (no external API needed)
        _ = DetectCountryAsync();

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

                        // Always stay closed on page load — user opens manually or via notification
                        _state = ChatState.Closed;
                    }
                    else
                    {
                        // No saved read pointer — mark all as read, stay closed
                        _lastReadMessageId = _messages.Count > 0 ? _messages.Max(m => m.Id) : 0;
                        _unreadCount = 0;
                        _ = SaveLastReadAsync();
                        _state = ChatState.Closed;
                    }

                    // Restore admin-read pointer (for ✓✓ on user's outgoing messages)
                    ProtectedBrowserStorageResult<int> adminReadResult = await LocalStorage.GetAsync<int>("chatAdminReadId");
                    if (adminReadResult.Success && adminReadResult.Value > 0)
                    {
                        _adminReadUpToId = adminReadResult.Value;
                    }

                    await ConnectHubAsync();

                    // Re-subscribe push for returning visitors — only if already granted
                    // (avoids showing permission dialog unexpectedly on return visit).
                    // This refreshes a stale/cleared subscription silently.
                    _ = ResubscribeIfGrantedAsync(_sessionId.Value);

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

    private async Task ResubscribeIfGrantedAsync(int sessionId)
    {
        try
        {
            string permission = await JS.InvokeAsync<string>("pushUtils.getPermission");
            if (permission == "granted")
            {
                await SubscribePushAsync(sessionId, isAdmin: false);
            }
        }
        catch { /* non-critical */ }
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
            // Ask for push permission — fire-and-forget, non-critical
            _ = SubscribePushAsync(session.Id, isAdmin: false);
            // Check if previously denied so we can show the hint banner
            _ = CheckPushPermissionAsync();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task CheckPushPermissionAsync()
    {
        try
        {
            string permission = await JS.InvokeAsync<string>("pushUtils.getPermission");
            if (permission == "denied")
            {
                _pushHelpUrl = await JS.InvokeAsync<string>("pushUtils.getNotificationHelpUrl");
                _pushDenied = true;
            }
            await InvokeAsync(StateHasChanged);
        }
        catch { /* non-critical */ }
    }

    private async Task SubscribePushAsync(int? chatSessionId, bool isAdmin)
    {
        try
        {
            string? subscriptionJson = await JS.InvokeAsync<string?>("pushUtils.subscribe");
            if (string.IsNullOrEmpty(subscriptionJson))
            {
                return;
            }

            using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(subscriptionJson);
            string endpoint = doc.RootElement.GetProperty("endpoint").GetString() ?? "";
            string p256dh   = doc.RootElement.GetProperty("keys").GetProperty("p256dh").GetString() ?? "";
            string auth     = doc.RootElement.GetProperty("keys").GetProperty("auth").GetString() ?? "";

            await PushSvc.SaveSubscriptionAsync(endpoint, p256dh, auth, chatSessionId, isAdmin);
        }
        catch
        {
            // Push subscription is non-critical — silently ignore errors
        }
    }

    private async Task SendMessage()
    {
        if (_sending || !_sessionId.HasValue)
        {
            return;
        }

        // Read value directly from DOM — no Blazor bind round-trip
        string raw = await JS.InvokeAsync<string>("chatUtils.getMsgInput");
        string content = raw?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        // Clear input immediately via JS — no re-render needed
        await JS.InvokeVoidAsync("chatUtils.clearMsgInput");
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
            // Revert on failure — restore text to input
            _messages.Remove(optimistic);
            await JS.InvokeVoidAsync("chatUtils.setMsgInput", content);
        }
        finally
        {
            _sending = false;
        }
    }

    // Called by SW postMessage / ?chat=open URL when visitor clicks a push notification
    [Microsoft.JSInterop.JSInvokable]
    public async Task OpenFromNotification()
    {
        if (_state == ChatState.Closed)
        {
            _state = _sessionId.HasValue ? ChatState.Chat : ChatState.Form;
            if (_state == ChatState.Chat)
            {
                SetUnreadDivider();
                _unreadCount = 0;
                _ = SaveLastReadAsync();
                _pendingScrollToUnread = true;
            }
            await InvokeAsync(StateHasChanged);
        }
    }

    // Called from JS (chatUtils.onMsgInput) via DotNet.invokeMethod — no Blazor bind needed
    [Microsoft.JSInterop.JSInvokable]
    public async Task SendMessageFromJs() => await SendMessage();

    [Microsoft.JSInterop.JSInvokable]
    public void OnTypingInput()
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

        // Retry up to 4 times with exponential backoff — handles transient EAGAIN (resource unavailable)
        int[] delays = [500, 1500, 3000, 6000];
        for (int attempt = 0; attempt <= delays.Length; attempt++)
        {
            try
            {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("JoinSession", _sessionId.Value.ToString());
                return; // success
            }
            catch when (attempt < delays.Length)
            {
                await Task.Delay(delays[attempt]);
            }
        }
    }

    protected override void OnInitialized()
    {
        Tz.OnTimezoneSet += OnTimezoneUpdated;
        Lang.OnChange += OnLangChanged;
    }

    private void OnTimezoneUpdated()
    {
        // Timezone only affects timestamps inside the open chat — skip re-render if closed
        if (_state != ChatState.Closed)
        {
            _ = InvokeAsync(StateHasChanged);
        }
    }
    private void OnLangChanged() => InvokeAsync(StateHasChanged);

    // Map IANA timezone → ISO country code
    private static readonly Dictionary<string, string> TimezoneToCountry = new()
    {
        ["Asia/Ho_Chi_Minh"] = "VN", ["Asia/Saigon"] = "VN",
        ["Asia/Hanoi"] = "VN",
        ["America/New_York"] = "US", ["America/Chicago"] = "US",
        ["America/Denver"] = "US", ["America/Los_Angeles"] = "US",
        ["America/Phoenix"] = "US", ["America/Anchorage"] = "US",
        ["Pacific/Honolulu"] = "US",
        ["Europe/London"] = "GB",
        ["Australia/Sydney"] = "AU", ["Australia/Melbourne"] = "AU",
        ["Australia/Brisbane"] = "AU", ["Australia/Perth"] = "AU",
        ["America/Toronto"] = "CA", ["America/Vancouver"] = "CA",
        ["Europe/Paris"] = "FR",
        ["Europe/Berlin"] = "DE",
        ["Asia/Tokyo"] = "JP",
        ["Asia/Seoul"] = "KR",
        ["Asia/Shanghai"] = "CN", ["Asia/Beijing"] = "CN",
        ["Asia/Singapore"] = "SG",
        ["Asia/Bangkok"] = "TH",
        ["Asia/Manila"] = "PH",
        ["Asia/Jakarta"] = "ID",
        ["Asia/Kuala_Lumpur"] = "MY",
        ["Asia/Kolkata"] = "IN",
        ["Pacific/Auckland"] = "NZ",
        ["America/Sao_Paulo"] = "BR",
        ["America/Mexico_City"] = "MX",
        ["Europe/Amsterdam"] = "NL",
        ["Europe/Rome"] = "IT",
        ["Europe/Madrid"] = "ES",
        ["Europe/Moscow"] = "RU",
        ["Asia/Dubai"] = "AE",
        ["Asia/Riyadh"] = "SA",
    };

    private async Task DetectCountryAsync()
    {
        try
        {
            // Call ipinfo.io from the browser (user's own IP — avoids server-side proxy issues)
            string code = await JS.InvokeAsync<string>("chatUtils.detectCountry");
            if (!string.IsNullOrWhiteSpace(code) && Countries.Any(c => c.RegionCode == code))
            {
                _selectedRegion = code;
                // Only re-render if form is currently visible — avoids a flash
                // when ipinfo.io returns while the chat panel is still closed.
                // When user opens the form later, _selectedRegion is already correct.
                if (_state != ChatState.Closed)
                {
                    await InvokeAsync(StateHasChanged);
                }
                return;
            }
        }
        catch
        {
            // ignore — fall through to timezone
        }

        // Fallback: timezone-based detection
        DetectCountryFromTimezone();
    }

    private void DetectCountryFromTimezone()
    {
        string tzId = Tz.Timezone.Id;
        if (TimezoneToCountry.TryGetValue(tzId, out string? region) &&
            Countries.Any(c => c.RegionCode == region))
        {
            _selectedRegion = region;
        }
    }

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
