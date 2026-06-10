using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminSkills : ComponentBase, IDisposable
{
    [Inject] private SkillApiClient SkillClient { get; set; } = default!;
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
    private List<Skill> _skills = [];
    private Skill Editing = new();
    private bool ShowForm;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private IQueryable<Skill> Filtered => _skills.AsQueryable()
        .Where(s => string.IsNullOrEmpty(_search) ||
                    s.Name.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    s.Category.Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _skills = await SkillClient.GetAllAsync();
        _loading = false;
    }

    private void OpenAdd() { Editing = new Skill(); ShowForm = true; }
    private void OpenEdit(Skill s) { Editing = new Skill { Id = s.Id, Name = s.Name, Category = s.Category, Icon = s.Icon, Proficiency = s.Proficiency, Order = s.Order }; ShowForm = true; }
    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await SkillClient.SaveAsync(Editing);
        ShowForm = false;
        await LoadAsync();
        Toast.Show(Loc.T("Saved successfully"));
    }

    private async Task Delete(int id)
    {
        await SkillClient.DeleteAsync(id);
        Toast.Show(Loc.T("Deleted"));
        await LoadAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
