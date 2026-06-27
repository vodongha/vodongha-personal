using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;
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
    private int _total;
    private int _unreadContacts;
    private ContactMessage? Selected;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<ContactMessage> _grid = default!;

    private Guid _deleteId;
    private bool _confirmShow;
    private void ConfirmDelete(Guid rid) { _deleteId = rid; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private async ValueTask<GridItemsProviderResult<ContactMessage>> LoadGrid(GridItemsProviderRequest<ContactMessage> req)
    {
        int size = req.Count ?? _pagination.ItemsPerPage;
        (string? sortBy, string sortDir) = AdminGrid.MapSort(req);
        PagedResult<ContactMessage> res = await ContactClient.GetPagedAsync(AdminGrid.PageOf(req, _pagination.ItemsPerPage), size, _search, sortBy, sortDir);
        _total = res.Total;
        if (_loading) { _loading = false; }
        _ = InvokeAsync(StateHasChanged);
        return GridItemsProviderResult.From(res.Items, res.Total);
    }

    private async Task OnSearch(ChangeEventArgs e)
    {
        _search = e.Value?.ToString() ?? "";
        await _pagination.SetCurrentPageIndexAsync(0);
        if (_grid is not null) { await _grid.RefreshDataAsync(); }
    }

    private async Task SetPageSize(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int size))
        {
            _pagination.ItemsPerPage = size;
            await _pagination.SetCurrentPageIndexAsync(0);
            if (_grid is not null) { await _grid.RefreshDataAsync(); }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        _unreadContacts = await ContactClient.GetUnreadCountAsync();
    }

    private async Task OpenMessage(ContactMessage msg)
    {
        Selected = msg;
        if (!msg.IsRead)
        {
            await ContactClient.MarkReadAsync(msg.Rid);
            msg.IsRead = true;
            _unreadContacts = await ContactClient.GetUnreadCountAsync();
            if (_grid is not null) { await _grid.RefreshDataAsync(); }
        }
    }

    private void CloseMessage() => Selected = null;

    private async Task MarkAllRead()
    {
        await ContactClient.MarkAllReadAsync();
        _unreadContacts = 0;
        if (_grid is not null) { await _grid.RefreshDataAsync(); }
        Toast.Show(Loc.T("Marked all as read"));
    }

    private async Task Delete(Guid rid)
    {
        await ContactClient.DeleteAsync(rid);
        if (Selected?.Rid == rid) { Selected = null; }
        _unreadContacts = await ContactClient.GetUnreadCountAsync();
        if (_grid is not null) { await _grid.RefreshDataAsync(); }
        Toast.Show(Loc.T("Deleted"));
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
