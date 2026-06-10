using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using VodonghaPersonal.Data;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AppSecretsService Secrets { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private int _unreadChatCount;
    private int _unreadMessagesCount;
    private string _search = "";

    private string _menuOrder = "[]";
    private DotNetObjectReference<Dashboard>? _dotNetRef;

    private const string MenuPrefKey = "_pref.dashboard.menu";

    private List<MenuItem> AllItems => [
        new("dash-dashboard", "/admin/dashboard", "bi bi-speedometer2", "Dashboard"),
        new("dash-analytics", "/admin/analytics", "bi bi-graph-up",     "Analytics"),
        new("dash-skills",    "/admin/skills",    "bi bi-bar-chart",    Loc.T("Skills")),
        new("dash-projects",  "/admin/projects",  "bi bi-folder2",      Loc.T("Projects")),
        new("dash-education", "/admin/education", "bi bi-mortarboard",  Loc.T("Education")),
        new("dash-experience","/admin/experience","bi bi-briefcase",    Loc.T("Experience")),
        new("dash-blog",      "/admin/blog",      "bi bi-file-text",    Loc.T("Blog")),
        new("dash-messages",  "/admin/contacts",  "bi bi-envelope",     Loc.T("Messages"),  _unreadMessagesCount, _unreadMessagesCount > 0 ? "admin-card--unread" : ""),
        new("dash-chats",     "/admin/chats",     "bi bi-chat-dots",    Loc.T("Chats"),     _unreadChatCount,     _unreadChatCount     > 0 ? "admin-card--unread" : ""),
        new("dash-cv",        "/admin/cv",        "bi bi-file-earmark-person", Loc.T("CV")),
        new("dash-health",    "/admin/health",    "bi bi-activity",     Loc.T("Health")),
        new("dash-costs",     "/admin/costs",     "bi bi-currency-dollar", Loc.T("Costs")),
        new("dash-api-keys",  "/admin/api-keys",  "bi bi-key",          Loc.T("API Keys")),
        new("dash-settings",  "/admin/settings",  "bi bi-gear",         Loc.T("Settings")),
    ];

    private List<MenuItem> FilteredItems =>
        string.IsNullOrWhiteSpace(_search)
            ? AllItems
            : AllItems.Where(x => x.Label.Contains(_search, StringComparison.OrdinalIgnoreCase)).ToList();

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _dotNetRef = DotNetObjectReference.Create(this);
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        _unreadMessagesCount = await db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        try
        {
            // Desktop users get redirected to the Dashboard page — /admin is mobile-only
            int width = await JS.InvokeAsync<int>("eval", "window.innerWidth");
            if (width > 768)
            {
                Nav.NavigateTo("/admin/dashboard", replace: true);
                return;
            }

            _menuOrder = await Secrets.GetValueAsync(MenuPrefKey) ?? "[]";
            await InvokeAsync(StateHasChanged);
            await JS.InvokeVoidAsync("initSortableCards", "admin-dash-cards", _dotNetRef, MenuPrefKey);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException)  { }
        catch (OperationCanceledException) { }
    }

    [JSInvokable]
    public async Task SaveCardOrder(string prefKey, string[] ids)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(ids);
        await Secrets.SaveAsync(prefKey, json);
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public async ValueTask DisposeAsync()
    {
        Loc.OnChanged -= OnLangChanged;
        _dotNetRef?.Dispose();
        await ValueTask.CompletedTask;
    }

    public sealed record MenuItem(
        string CardId,
        string Href,
        string Icon,
        string Label,
        int    BadgeCount = 0,
        string ExtraClass = "");
}
