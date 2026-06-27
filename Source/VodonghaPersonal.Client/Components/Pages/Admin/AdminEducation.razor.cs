using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminEducation : ComponentBase, IDisposable
{
    [Inject] private EducationApiClient EduClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private Guid _deleteId;
    private bool _confirmShow;

    private void ConfirmDelete(Guid rid) { _deleteId = rid; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private bool _loading = true;
    private Education Editing = new();
    private bool ShowForm;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<Education> _grid = default!;

    private async ValueTask<GridItemsProviderResult<Education>> LoadGrid(GridItemsProviderRequest<Education> req)
    {
        int size = req.Count ?? _pagination.ItemsPerPage;
        (string? sortBy, string sortDir) = AdminGrid.MapSort(req);
        PagedResult<Education> res = await EduClient.GetPagedAsync(AdminGrid.PageOf(req, _pagination.ItemsPerPage), size, _search, sortBy, sortDir);
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

    private void OpenAdd() { Editing = new Education(); ShowForm = true; }
    private void OpenEdit(Education e) { Editing = new Education { Id = e.Id, Rid = e.Rid, School = e.School, Degree = e.Degree, Field = e.Field, WebsiteUrl = e.WebsiteUrl, StartYear = e.StartYear, EndYear = e.EndYear, Description = e.Description, DescriptionEn = e.DescriptionEn, Order = e.Order }; ShowForm = true; }
    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await EduClient.SaveAsync(Editing);
        ShowForm = false;
        await _grid.RefreshDataAsync();
        Toast.Show(Loc.T("Saved successfully"));
    }

    private async Task Delete(Guid rid)
    {
        await EduClient.DeleteAsync(rid);
        Toast.Show(Loc.T("Deleted"));
        await _grid.RefreshDataAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
