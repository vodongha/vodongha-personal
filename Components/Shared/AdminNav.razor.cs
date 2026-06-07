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

    private int _unreadChatCount;
    private int _unreadMessagesCount;
    private bool _menuOpen;

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
                string? stored = await JS.InvokeAsync<string?>("localStorage.getItem", "adminLang");
                if (stored == "EN" || stored == "VI")
                {
                    Loc.SetLang(stored);
                }
            }
            catch { }
        }
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
