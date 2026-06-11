using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminContacts : ComponentBase, IDisposable
{
    [Inject] private ContactApiClient ContactClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private bool _loading = true;
    private List<ContactMessage> Messages = [];
    private ContactMessage? Selected;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private int UnreadCount => Messages.Count(m => !m.IsRead);

    private Guid _deleteId;
    private bool _confirmShow;
    private void ConfirmDelete(Guid id) { _deleteId = id; _confirmShow = true; }
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
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        Messages = await ContactClient.GetAllAsync();
        _loading = false;
    }

    private async Task OpenMessage(ContactMessage msg)
    {
        Selected = msg;
        if (!msg.IsRead)
        {
            await ContactClient.MarkReadAsync(msg.Id);
            msg.IsRead = true;
        }
    }

    private void CloseMessage() => Selected = null;

    private async Task MarkAllRead()
    {
        await ContactClient.MarkAllReadAsync();
        foreach (ContactMessage msg in Messages) { msg.IsRead = true; }
        Toast.Show(Loc.T("Marked all as read"));
    }

    private async Task Delete(Guid id)
    {
        await ContactClient.DeleteAsync(id);
        Messages.RemoveAll(m => m.Id == id);
        if (Selected?.Id == id) { Selected = null; }
        Toast.Show(Loc.T("Deleted"));
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
