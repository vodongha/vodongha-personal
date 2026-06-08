using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminBlog : ComponentBase, IDisposable
{
    [Inject] private BlogService BlogSvc { get; set; } = default!;
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
    private List<BlogPost> _posts = [];
    private BlogPost Editing = new();
    private bool ShowForm;
    private bool _isNew;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };

    private IQueryable<BlogPost> Filtered => _posts.AsQueryable()
        .Where(p => string.IsNullOrEmpty(_search) ||
                    p.Title.Contains(_search, StringComparison.OrdinalIgnoreCase) ||
                    (p.Tags ?? "").Contains(_search, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        _unreadChatCount = await ChatSvc.GetUnreadCountAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _posts = await BlogSvc.GetAllAsync();
        _loading = false;
    }

    private void OpenAdd() { Editing = new BlogPost { CreatedAt = DateTime.UtcNow }; _isNew = true; ShowForm = true; }

    private void OpenEdit(BlogPost post)
    {
        Editing = new BlogPost
        {
            Id = post.Id, Title = post.Title, TitleEn = post.TitleEn, Slug = post.Slug,
            Summary = post.Summary, SummaryEn = post.SummaryEn, Content = post.Content,
            ContentEn = post.ContentEn, Tags = post.Tags, CoverImageUrl = post.CoverImageUrl,
            IsPublished = post.IsPublished, CreatedAt = post.CreatedAt, UpdatedAt = post.UpdatedAt
        };
        _isNew = false;
        ShowForm = true;
    }

    private void CloseForm() { ShowForm = false; }

    private void OnTitleInput(ChangeEventArgs e)
    {
        Editing.Title = e.Value?.ToString() ?? string.Empty;
        if (_isNew) { Editing.Slug = GenerateSlug(Editing.Title); }
    }

    private static string GenerateSlug(string title)
    {
        string slug = title.ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[àáạảãăắặẳẵâấậẩẫ]", "a");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[èéẹẻẽêếệểễ]", "e");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ìíịỉĩ]", "i");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[òóọỏõôốộổỗơớợởỡ]", "o");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ùúụủũưứựửữ]", "u");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ỳýỵỷỹ]", "y");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[đ]", "d");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        return slug.Trim('-');
    }

    private async Task Save() { await BlogSvc.SaveAsync(Editing); ShowForm = false; await LoadAsync(); Toast.Show("Đã lưu bài viết thành công"); }

    private async Task Delete(int id) { await BlogSvc.DeleteAsync(id); await LoadAsync(); Toast.Show("Đã xoá bài viết"); }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
