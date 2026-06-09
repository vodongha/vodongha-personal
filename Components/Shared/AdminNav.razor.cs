using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Shared;

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
    private bool _collapsed;

    private static readonly Dictionary<string, string[]> _groupPaths = new()
    {
        ["Portfolio"]      = ["/admin/skills", "/admin/projects", "/admin/education", "/admin/experience", "/admin/blog", "/admin/cv"],
        ["Communication"]  = ["/admin/contacts", "/admin/chats"],
        ["Insights"]       = ["/admin/analytics", "/admin/health", "/admin/costs"],
        ["System"]         = ["/admin/api-keys", "/admin/settings"],
    };
    private HashSet<string> _openGroups = [];

    private async Task ToggleSidebar()
    {
        _collapsed = !_collapsed;
        try { await JS.InvokeVoidAsync("localStorage.setItem", "admin-sidebar-collapsed", _collapsed ? "true" : "false"); }
        catch { }
    }

    private async Task OnGroupHeaderClick(string group)
    {
        if (_collapsed)
        {
            _collapsed = false;
            try { await JS.InvokeVoidAsync("localStorage.setItem", "admin-sidebar-collapsed", "false"); }
            catch { }
            _openGroups.Add(group);
        }
        else
        {
            ToggleGroup(group);
        }
    }

    private void ToggleGroup(string group)
    {
        if (!_openGroups.Remove(group))
        {
            _openGroups.Add(group);
        }
    }

    private bool IsGroupOpen(string group) => _openGroups.Contains(group);

    private bool IsGroupActive(string group) =>
        _groupPaths.TryGetValue(group, out string[]? paths) && paths.Any(p => IsActive(p));

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;

        // Auto-open the group that contains the current page
        foreach (KeyValuePair<string, string[]> kvp in _groupPaths)
        {
            if (kvp.Value.Any(p => IsActive(p)))
            {
                _openGroups.Add(kvp.Key);
                break;
            }
        }

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
                string? savedCollapsed = await JS.InvokeAsync<string?>("localStorage.getItem", "admin-sidebar-collapsed");
                _collapsed = savedCollapsed == "true";

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
