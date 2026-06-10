using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Components.Shared;

public partial class AdminNav : ComponentBase, IAsyncDisposable
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private ContactApiClient ContactClient { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private SettingsApiClient SettingsClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private int _unreadMessagesCount;
    private bool _menuOpen;
    private string _theme = "dark";
    private bool _collapsed;

    private static readonly Dictionary<string, string[]> _groupPaths = new()
    {
        ["Portfolio"] = ["/admin/skills", "/admin/projects", "/admin/education", "/admin/experience", "/admin/blog", "/admin/cv"],
        ["Communication"] = ["/admin/contacts", "/admin/chats"],
        ["Insights"] = ["/admin/analytics", "/admin/health", "/admin/costs"],
        ["System"] = ["/admin/api-keys", "/admin/settings"],
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

        foreach (KeyValuePair<string, string[]> kvp in _groupPaths)
        {
            if (kvp.Value.Any(p => IsActive(p)))
            {
                _openGroups.Add(kvp.Key);
                break;
            }
        }

        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        List<VodonghaPersonal.Shared.Models.ContactMessage> contacts = await ContactClient.GetAllAsync();
        _unreadMessagesCount = contacts.Count(m => !m.IsRead);
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

                Dictionary<string, string> settings = await SettingsClient.GetAllAsync();
                string? dbTheme = settings.GetValueOrDefault("admin.theme");
                if (dbTheme == "light" || dbTheme == "dark")
                {
                    _theme = dbTheme;
                }
                else
                {
                    _theme = await JS.InvokeAsync<string>("getUserTheme");
                }
                await JS.InvokeVoidAsync("setTheme", _theme);
                await JS.InvokeVoidAsync("localStorage.setItem", "theme", _theme);
                StateHasChanged();

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
            string p256dh = doc.RootElement.GetProperty("keys").GetProperty("p256dh").GetString() ?? "";
            string auth = doc.RootElement.GetProperty("keys").GetProperty("auth").GetString() ?? "";

            await Http.PostAsJsonAsync("/api/push/subscribe", new { endpoint, p256dh, auth, chatSessionId = (int?)null });
        }
        catch { }
    }

    private async Task ToggleTheme()
    {
        try
        {
            _theme = await JS.InvokeAsync<string>("toggleTheme");
            Dictionary<string, string> current = await SettingsClient.GetAllAsync();
            current["admin.theme"] = _theme;
            await SettingsClient.SaveAllAsync(current);
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
        if (qIdx >= 0)
        {
            current = current[..qIdx];
        }

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
