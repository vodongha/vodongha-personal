using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class AdminContacts : ComponentBase, IDisposable
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatService ChatSvc { get; set; } = default!;

    private int _unreadChatCount;
    private bool _loading = true;
    private List<ContactMessage> Messages = [];
    private ContactMessage? Selected;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private int UnreadCount => Messages.Count(m => !m.IsRead);

    private int _deleteId;
    private bool _confirmShow;
    private void ConfirmDelete(int id) { _deleteId = id; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private async Task SetPageSize(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int size))
        {
            _pagination.ItemsPerPage = size;
            await _pagination.SetCurrentPageIndexAsync(0);
        }
    }

    private IQueryable<ContactMessage> Filtered => Messages.AsQueryable()
        .Where(m => string.IsNullOrEmpty(_search) ||
                    m.Name.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    m.Subject.Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        Messages = await db.ContactMessages.OrderByDescending(m => m.SentAt).ToListAsync();
        _loading = false;
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
        await db.ContactMessages.Where(m => !m.IsRead).ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        foreach (ContactMessage msg in Messages) { msg.IsRead = true; }
        Toast.Show("Đã đánh dấu tất cả đã đọc");
    }

    private async Task Delete(int id)
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        await db.ContactMessages.Where(m => m.Id == id).ExecuteDeleteAsync();
        Messages.RemoveAll(m => m.Id == id);
        if (Selected?.Id == id) { Selected = null; }
        Toast.Show("Đã xoá tin nhắn");
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
