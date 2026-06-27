using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminProjects : ComponentBase, IDisposable
{
    [Inject] private ProjectApiClient ProjectClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private Guid _deleteId;
    private bool _confirmShow;

    private void ConfirmDelete(Guid rid) { _deleteId = rid; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private bool _loading = true;
    private List<Project> _pageItems = [];
    private int _total;
    private Project Editing = new();
    private bool ShowForm;
    private string _search = "";
    private int _pageSize = 10;
    private int _page;

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_total / (double)_pageSize));

    // Drag indices are local to the current page (server-side paging only ever loads one page).
    private int _dragIndex = -1;
    private int _dropIndex = -1;

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        await LoadPageAsync();
    }

    private async Task LoadPageAsync()
    {
        PagedResult<Project> res = await ProjectClient.GetPagedAsync(_page, _pageSize, _search);
        _pageItems = res.Items;
        _total = res.Total;
        // A delete can leave us past the last page — step back and refetch.
        if (_page > 0 && _page > TotalPages - 1)
        {
            _page = TotalPages - 1;
            res = await ProjectClient.GetPagedAsync(_page, _pageSize, _search);
            _pageItems = res.Items;
            _total = res.Total;
        }
        _loading = false;
    }

    private async Task OnSearch(ChangeEventArgs e)
    {
        _search = e.Value?.ToString() ?? "";
        _page = 0;
        await LoadPageAsync();
    }

    private async Task GoPage(int p)
    {
        _page = Math.Clamp(p, 0, TotalPages - 1);
        await LoadPageAsync();
    }

    private async Task OnPageSizeChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int s)) { _pageSize = s; _page = 0; await LoadPageAsync(); }
    }

    private void DragStart(int localIndex) => _dragIndex = localIndex;
    private void DragOver(int localIndex) => _dropIndex = localIndex;
    private void DragEnd() { _dragIndex = -1; _dropIndex = -1; }

    private string DragClass(int localIndex)
    {
        if (localIndex == _dragIndex) { return "admin-row--dragging"; }
        if (localIndex == _dropIndex) { return "admin-row--dragover"; }
        return "";
    }

    private async Task Drop(int targetLocalIndex)
    {
        if (_dragIndex < 0 || _dragIndex == targetLocalIndex) { DragEnd(); return; }

        // Permute only this page's Order slots among the reordered rows, so the global
        // ordering (other pages) is untouched.
        List<int> orders = _pageItems.Select(p => p.Order).OrderBy(o => o).ToList();
        Project moved = _pageItems[_dragIndex];
        _pageItems.RemoveAt(_dragIndex);
        _pageItems.Insert(targetLocalIndex, moved);

        List<ProjectOrderItem> pairs = [];
        for (int k = 0; k < _pageItems.Count; k++)
        {
            _pageItems[k].Order = orders[k];
            pairs.Add(new ProjectOrderItem(_pageItems[k].Rid, orders[k]));
        }
        _dragIndex = -1; _dropIndex = -1;
        await ProjectClient.SaveReorderAsync(pairs);
        Toast.Show(Loc.T("Order saved"));
    }

    private void OpenAdd() { Editing = new Project { CreatedAt = DateTime.UtcNow }; ShowForm = true; }

    private void OpenEdit(Project p)
    {
        Editing = new Project
        {
            Id = p.Id,
            Rid = p.Rid,
            Title = p.Title,
            Description = p.Description,
            DescriptionEn = p.DescriptionEn,
            Technologies = p.Technologies,
            ImageUrl = p.ImageUrl,
            GitHubUrl = p.GitHubUrl,
            LiveUrl = p.LiveUrl,
            IsFeatured = p.IsFeatured,
            Order = p.Order,
            CreatedAt = p.CreatedAt
        };
        ShowForm = true;
    }

    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await ProjectClient.SaveAsync(Editing);
        ShowForm = false;
        await LoadPageAsync();
        Toast.Show(Loc.T("Saved successfully"));
    }

    private async Task Delete(Guid rid)
    {
        await ProjectClient.DeleteAsync(rid);
        Toast.Show(Loc.T("Deleted"));
        await LoadPageAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
