using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminEducation : ComponentBase, IDisposable
{
    [Inject] private EducationApiClient EduClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
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

    private bool _loading = true;
    private List<Education> _items = [];
    private Education Editing = new();
    private bool ShowForm;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private IQueryable<Education> Filtered => _items.AsQueryable()
        .Where(e => string.IsNullOrEmpty(_search) ||
                    e.School.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    e.Field.Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _items = await EduClient.GetAllAsync();
        _loading = false;
    }

    private void OpenAdd() { Editing = new Education(); ShowForm = true; }
    private void OpenEdit(Education e) { Editing = new Education { Id = e.Id, School = e.School, Degree = e.Degree, Field = e.Field, WebsiteUrl = e.WebsiteUrl, StartYear = e.StartYear, EndYear = e.EndYear, Description = e.Description, DescriptionEn = e.DescriptionEn, Order = e.Order }; ShowForm = true; }
    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await EduClient.SaveAsync(Editing);
        ShowForm = false;
        await LoadAsync();
        Toast.Show("Đã lưu thành công");
    }

    private async Task Delete(int id)
    {
        await EduClient.DeleteAsync(id);
        Toast.Show("Đã xoá");
        await LoadAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
