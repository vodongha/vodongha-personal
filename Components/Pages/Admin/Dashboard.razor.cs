using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class Dashboard : ComponentBase
{
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;

    private int _unreadChatCount;
    private int _unreadMessagesCount;

    protected override async Task OnInitializedAsync()
    {
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        _unreadMessagesCount = await db.ContactMessages.CountAsync(m => !m.IsRead);
    }
}
