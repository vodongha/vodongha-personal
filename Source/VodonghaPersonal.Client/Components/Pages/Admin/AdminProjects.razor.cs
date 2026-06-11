using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminProjects : ComponentBase, IDisposable
{
    [Inject] private ProjectApiClient ProjectClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private int _deleteId;
    private bool _confirmShow;

    private void ConfirmDelete(int id) { _deleteId = id; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private bool _loading = true;
    private List<Project> Projects = [];
    private Project Editing = new();
    private bool ShowForm;
    private string _search = "";
    private int _pageSize = 10;
    private int _page = 0;

    private List<Project> Filtered => string.IsNullOrEmpty(_search)
        ? Projects
        : Projects.Where(p => p.Title.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                               (p.Technologies ?? "").Contains(_search, StringComparison.OrdinalIgnoreCase)).ToList();

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(Filtered.Count / (double)_pageSize));
    private List<Project> Paged => Filtered.Skip(_page * _pageSize).Take(_pageSize).ToList();

    private void GoPage(int p) { _page = Math.Clamp(p, 0, TotalPages - 1); }
    private void ResetPage() { _page = 0; }
    private void OnPageSizeChange(ChangeEventArgs e) { if (int.TryParse(e.Value?.ToString(), out int s)) { _pageSize = s; _page = 0; } }

    private int _dragIndex = -1;
    private int _dropIndex = -1;

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatClient.GetUnreadCountAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        Projects = await ProjectClient.GetAllAsync();
        _loading = false;
    }

    private void DragStart(int localIndex) => _dragIndex = _page * _pageSize + localIndex;
    private void DragOver(int localIndex) => _dropIndex = _page * _pageSize + localIndex;
    private void DragEnd() { _dragIndex = -1; _dropIndex = -1; }

    private string DragClass(int localIndex)
    {
        int globalIndex = _page * _pageSize + localIndex;
        if (globalIndex == _dragIndex) { return "admin-row--dragging"; }
        if (globalIndex == _dropIndex) { return "admin-row--dragover"; }
        return "";
    }

    private async Task Drop(int targetLocalIndex)
    {
        int targetIndex = _page * _pageSize + targetLocalIndex;
        if (_dragIndex < 0 || _dragIndex == targetIndex) { DragEnd(); return; }
        Project dragged = Projects[_dragIndex];
        Projects.RemoveAt(_dragIndex);
        Projects.Insert(targetIndex, dragged);
        for (int i = 0; i < Projects.Count; i++) { Projects[i].Order = i + 1; }
        _dragIndex = -1; _dropIndex = -1;
        await ProjectClient.SaveOrderAsync(Projects.Select(p => p.Id).ToList());
        Toast.Show(Loc.T("Order saved"));
    }

    private void OpenAdd() { Editing = new Project { CreatedAt = DateTime.UtcNow }; ShowForm = true; }

    private void OpenEdit(Project p)
    {
        Editing = new Project
        {
            Id = p.Id,
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
        await LoadAsync();
        Toast.Show(Loc.T("Saved successfully"));
    }

    private async Task Delete(int id)
    {
        await ProjectClient.DeleteAsync(id);
        Toast.Show(Loc.T("Deleted"));
        await LoadAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
