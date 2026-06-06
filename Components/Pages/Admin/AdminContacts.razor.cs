using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminContacts : ComponentBase
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

    private List<ContactMessage> Messages = [];
    private ContactMessage? Selected;
    private int UnreadCount => Messages.Count(m => !m.IsRead);

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        Messages = await db.ContactMessages
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    private async Task OpenMessage(ContactMessage msg)
    {
        Selected = msg;
        if (!msg.IsRead)
        {
            await using AppDbContext db = await DbFactory.CreateDbContextAsync();
            ContactMessage? entity = await db.ContactMessages.FindAsync(msg.Id);
            if (entity is not null)
            {
                entity.IsRead = true;
                await db.SaveChangesAsync();
                msg.IsRead = true;
            }
        }
    }

    private void CloseMessage() => Selected = null;

    private async Task MarkAllRead()
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        await db.ContactMessages
            .Where(m => !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        foreach (ContactMessage msg in Messages)
        {
            msg.IsRead = true;
        }
        Toast.Show("Đã đánh dấu tất cả đã đọc");
    }

    private async Task Delete(int id)
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        await db.ContactMessages.Where(m => m.Id == id).ExecuteDeleteAsync();
        Messages.RemoveAll(m => m.Id == id);
        if (Selected?.Id == id)
        {
            Selected = null;
        }
        Toast.Show("Đã xoá tin nhắn");
    }
}
