using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Shared.Models;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminBlog : ComponentBase, IDisposable
{
    [Inject] private BlogApiClient BlogClient { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private ChatApiClient ChatClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _unreadChatCount;
    private Guid _deleteId;
    private bool _confirmShow;

    private void ConfirmDelete(Guid rid) { _deleteId = rid; _confirmShow = true; }
    private async Task ExecuteDelete() { _confirmShow = false; await Delete(_deleteId); }

    private bool _loading = true;
    private BlogPost Editing = new();
    private bool ShowForm;
    private bool _isNew;
    private string _search = "";
    private PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<BlogPost> _grid = default!;

    private async ValueTask<GridItemsProviderResult<BlogPost>> LoadGrid(GridItemsProviderRequest<BlogPost> req)
    {
        int size = req.Count ?? _pagination.ItemsPerPage;
        (string? sortBy, string sortDir) = AdminGrid.MapSort(req);
        PagedResult<BlogPost> res = await BlogClient.GetPagedAsync(AdminGrid.PageOf(req, _pagination.ItemsPerPage), size, _search, sortBy, sortDir);
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

    private void OpenAdd() { Editing = new BlogPost { CreatedAt = DateTime.UtcNow }; _isNew = true; ShowForm = true; }

    private void OpenEdit(BlogPost post)
    {
        Editing = new BlogPost
        {
            Id = post.Id,
            Rid = post.Rid,
            Title = post.Title,
            TitleEn = post.TitleEn,
            Slug = post.Slug,
            Summary = post.Summary,
            SummaryEn = post.SummaryEn,
            Content = post.Content,
            ContentEn = post.ContentEn,
            Tags = post.Tags,
            CoverImageUrl = post.CoverImageUrl,
            IsPublished = post.IsPublished,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
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

    private async Task Save() { await BlogClient.SaveAsync(Editing); ShowForm = false; await _grid.RefreshDataAsync(); Toast.Show(Loc.T("Saved successfully")); }
    private async Task Delete(Guid rid) { await BlogClient.DeleteAsync(rid); await _grid.RefreshDataAsync(); Toast.Show(Loc.T("Deleted")); }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
