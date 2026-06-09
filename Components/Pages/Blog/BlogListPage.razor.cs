using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Blog;

public partial class BlogListPage : ComponentBase, IDisposable
{
    [Inject] private BlogService BlogSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "page")] public int PageParam { get; set; } = 1;
    [SupplyParameterFromQuery(Name = "tag")]  public string? TagParam { get; set; }
    [SupplyParameterFromQuery(Name = "q")]    public string? QParam { get; set; }

    private const int PageSize = 6;

    private List<BlogPost>? _posts;
    private string _searchQuery = string.Empty;
    private string? _selectedTag;
    private int _currentPage = 1;

    private int CurrentPage => _currentPage;

    private IEnumerable<string> AllTags => _posts?
        .SelectMany(p => (p.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
        .Select(t => t.Trim())
        .Where(t => !string.IsNullOrEmpty(t))
        .Distinct()
        .OrderBy(t => t)
        ?? Enumerable.Empty<string>();

    private IEnumerable<BlogPost> FilteredPosts
    {
        get
        {
            if (_posts is null)
            {
                return Enumerable.Empty<BlogPost>();
            }

            IEnumerable<BlogPost> result = _posts;

            if (!string.IsNullOrEmpty(_selectedTag))
            {
                result = result.Where(p =>
                    p.Tags != null && p.Tags.Contains(_selectedTag, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.Trim().ToLowerInvariant();
                result = result.Where(p =>
                    (p.Title ?? "").ToLowerInvariant().Contains(q) ||
                    (p.TitleEn ?? "").ToLowerInvariant().Contains(q) ||
                    (p.Summary ?? "").ToLowerInvariant().Contains(q) ||
                    (p.SummaryEn ?? "").ToLowerInvariant().Contains(q));
            }

            return result;
        }
    }

    private int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredPosts.Count() / (double)PageSize));

    private IEnumerable<BlogPost> PagedPosts =>
        FilteredPosts.Skip((_currentPage - 1) * PageSize).Take(PageSize);

    protected override async Task OnInitializedAsync()
    {
        _posts = await BlogSvc.GetPublishedAsync();
        Lang.OnChange += StateHasChanged;
    }

    protected override void OnParametersSet()
    {
        // Sync URL query params → component state (runs on initial load and on browser back/forward)
        _currentPage = PageParam > 0 ? PageParam : 1;
        _selectedTag = TagParam;
        _searchQuery = QParam ?? string.Empty;
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        _searchQuery = e.Value?.ToString() ?? string.Empty;
        PushUrl(1);
    }

    private void SelectTag(string? tag)
    {
        _selectedTag = _selectedTag == tag ? null : tag;
        PushUrl(1);
    }

    private void GoToPage(int page)
    {
        _currentPage = Math.Clamp(page, 1, TotalPages);
        PushUrl(_currentPage);
    }

    /// <summary>Update the browser URL to reflect current filter + page state without full reload.</summary>
    private void PushUrl(int page)
    {
        _currentPage = page;

        Dictionary<string, object?> query = new()
        {
            ["page"] = page > 1 ? page : null,
            ["tag"]  = string.IsNullOrEmpty(_selectedTag) ? null : _selectedTag,
            ["q"]    = string.IsNullOrWhiteSpace(_searchQuery) ? null : _searchQuery.Trim(),
        };

        string url = Nav.GetUriWithQueryParameters("/blog", query!);
        Nav.NavigateTo(url, forceLoad: false);
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
