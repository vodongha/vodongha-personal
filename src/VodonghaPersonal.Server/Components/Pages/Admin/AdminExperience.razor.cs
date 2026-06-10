using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class AdminExperience : ComponentBase, IDisposable
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatService ChatSvc { get; set; } = default!;

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
    private List<Experience> _items = [];
    private Experience Editing = new();
    private bool ShowForm;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private IQueryable<Experience> Filtered => _items.AsQueryable()
        .Where(e => string.IsNullOrEmpty(_search) ||
                    e.Company.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    e.Role.Contains(_search, StringComparison.OrdinalIgnoreCase));

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
        _items = await db.Experiences.OrderBy(e => e.Order).ToListAsync();
        _loading = false;
    }

    private void OpenAdd() { Editing = new Experience { StartMonth = 1, StartYear = DateTime.Now.Year, IsCurrent = true }; ShowForm = true; }
    private void OpenEdit(Experience e) { Editing = new Experience { Id = e.Id, Company = e.Company, Role = e.Role, Location = e.Location, WebsiteUrl = e.WebsiteUrl, StartYear = e.StartYear, StartMonth = e.StartMonth, EndYear = e.EndYear, EndMonth = e.EndMonth, IsCurrent = e.IsCurrent, Description = e.Description, DescriptionEn = e.DescriptionEn, Order = e.Order }; ShowForm = true; }
    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        if (Editing.IsCurrent) { Editing.EndYear = null; Editing.EndMonth = null; }
        if (Editing.Id == 0) { db.Experiences.Add(Editing); }
        else { db.Experiences.Update(Editing); }
        await db.SaveChangesAsync();
        ShowForm = false;
        await LoadAsync();
        Toast.Show("Đã lưu thành công");
    }

    private async Task Delete(int id)
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        Experience? e = await db.Experiences.FindAsync(id);
        if (e != null) { db.Experiences.Remove(e); await db.SaveChangesAsync(); Toast.Show("Đã xoá"); }
        await LoadAsync();
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
