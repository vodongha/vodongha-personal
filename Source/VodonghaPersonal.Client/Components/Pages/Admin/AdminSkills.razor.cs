using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminSkills : ComponentBase, IDisposable
{
    [Inject] private SkillApiClient SkillClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private Guid _deleteId;
    private bool _confirmShow;

    private void ConfirmDelete(Guid rid) { _deleteId = rid; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private bool _loading = true;
    private Skill Editing = new();
    private bool ShowForm;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<Skill> _grid = default!;

    // Server-side: QuickGrid asks for one page; the DB does search/sort/paginate.
    private async ValueTask<GridItemsProviderResult<Skill>> LoadGrid(GridItemsProviderRequest<Skill> req)
    {
        int size = req.Count ?? _pagination.ItemsPerPage;
        (string? sortBy, string sortDir) = AdminGrid.MapSort(req);
        PagedResult<Skill> res = await SkillClient.GetPagedAsync(AdminGrid.PageOf(req, _pagination.ItemsPerPage), size, _search, sortBy, sortDir);
        if (_loading) { _loading = false; _ = InvokeAsync(StateHasChanged); }
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
    }

    private void OpenAdd() { Editing = new Skill(); ShowForm = true; }
    private void OpenEdit(Skill s) { Editing = new Skill { Id = s.Id, Rid = s.Rid, Name = s.Name, Category = s.Category, Icon = s.Icon, Proficiency = s.Proficiency, Order = s.Order }; ShowForm = true; }
    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await SkillClient.SaveAsync(Editing);
        ShowForm = false;
        await _grid.RefreshDataAsync();
        Toast.Show(Loc.T("Saved successfully"));
    }

    private async Task Delete(Guid rid)
    {
        await SkillClient.DeleteAsync(rid);
        Toast.Show(Loc.T("Deleted"));
        await _grid.RefreshDataAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
