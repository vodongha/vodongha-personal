using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using vodongha.Data;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class AdminNav : ComponentBase, IAsyncDisposable
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private SiteSettingService SettingSvc { get; set; } = default!;
    [Inject] private PushNotificationService PushSvc { get; set; } = default!;

    private int _unreadChatCount;
    private int _unreadMessagesCount;
    private bool _menuOpen;
    private string _theme = "dark";

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        _unreadMessagesCount = await db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                string? storedLang = await JS.InvokeAsync<string?>("localStorage.getItem", "adminLang");
                if (storedLang == "EN" || storedLang == "VI")
                {
                    Loc.SetLang(storedLang);
                }

                // Read from DB first (admin explicit choice), fall back to OS preference via JS.
                string? dbTheme = await SettingSvc.GetAsync("admin.theme");
                if (dbTheme == "light" || dbTheme == "dark")
                {
                    _theme = dbTheme;
                }
                else
                {
                    // No admin preference saved yet — use OS/system preference.
                    _theme = await JS.InvokeAsync<string>("getUserTheme");
                }
                await JS.InvokeVoidAsync("setTheme", _theme);
                // Keep localStorage in sync so the public site matches.
                await JS.InvokeVoidAsync("localStorage.setItem", "theme", _theme);
                StateHasChanged();

                // Subscribe admin device for push notifications (non-critical)
                _ = SubscribeAdminPushAsync();
            }
            catch { }
        }
    }

    private async Task SubscribeAdminPushAsync()
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

            await PushSvc.SaveSubscriptionAsync(endpoint, p256dh, auth, chatSessionId: null, isAdmin: true);
        }
        catch
        {
            // Non-critical — silently ignore
        }
    }

    private async Task ToggleTheme()
    {
        try
        {
            // Read actual data-theme from DOM and flip — avoids the race where _theme
            // hasn't been synced from DB yet when the user clicks the toggle button.
            _theme = await JS.InvokeAsync<string>("toggleTheme");
            // Persist explicit admin choice to DB (survives browser clears / other devices).
            await SettingSvc.SetAsync("admin.theme", _theme);
            StateHasChanged();
        }
        catch { }
    }

    private async Task ToggleLang()
    {
        Loc.Toggle();
        try { await JS.InvokeVoidAsync("localStorage.setItem", "adminLang", Loc.Lang); }
        catch { }
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    private bool IsActive(string path, bool exact = false)
    {
        string current = "/" + Nav.ToBaseRelativePath(Nav.Uri).TrimStart('/');
        int qIdx = current.IndexOf('?');
        if (qIdx >= 0) current = current[..qIdx];

        if (exact)
        {
            return string.Equals(current, path, StringComparison.OrdinalIgnoreCase);
        }

        return current.StartsWith(path, StringComparison.OrdinalIgnoreCase);
    }

    private void ToggleMenu() => _menuOpen = !_menuOpen;
    private void CloseMenu() => _menuOpen = false;

    public async ValueTask DisposeAsync()
    {
        Loc.OnChanged -= OnLangChanged;
        await Task.CompletedTask;
    }
}
