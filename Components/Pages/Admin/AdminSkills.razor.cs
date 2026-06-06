using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminSkills : ComponentBase
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

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

    private List<Skill> _skills = [];
    private Skill Editing = new();
    private bool ShowForm;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private IQueryable<Skill> Filtered => _skills.AsQueryable()
        .Where(s => string.IsNullOrEmpty(_search) ||
                    s.Name.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    s.Category.Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        _skills = await db.Skills.OrderBy(s => s.Order).ToListAsync();
    }

    private void OpenAdd() { Editing = new Skill(); ShowForm = true; }
    private void OpenEdit(Skill s) { Editing = new Skill { Id = s.Id, Name = s.Name, Category = s.Category, Icon = s.Icon, Proficiency = s.Proficiency, Order = s.Order }; ShowForm = true; }
    private void CloseForm() { ShowForm = false; }

    private async Task Save()
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        if (Editing.Id == 0) { db.Skills.Add(Editing); }
        else { db.Skills.Update(Editing); }
        await db.SaveChangesAsync();
        ShowForm = false;
        await LoadAsync();
        Toast.Show("Đã lưu thành công");
    }

    private async Task Delete(int id)
    {
        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        Skill? skill = await db.Skills.FindAsync(id);
        if (skill != null) { db.Skills.Remove(skill); await db.SaveChangesAsync(); Toast.Show("Đã xoá"); }
        await LoadAsync();
    }
}
