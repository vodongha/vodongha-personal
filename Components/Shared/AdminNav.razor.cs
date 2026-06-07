using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class AdminNav : ComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;

    private int _unreadChatCount;
    private int _unreadMessagesCount;
    private bool _menuOpen;

    protected override async Task OnInitializedAsync()
    {
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        _unreadMessagesCount = await db.ContactMessages.CountAsync(m => !m.IsRead);
    }

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
}
