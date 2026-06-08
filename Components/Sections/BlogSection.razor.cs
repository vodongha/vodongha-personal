using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Sections;

public partial class BlogSection : ComponentBase, IDisposable
{
    [Inject] private BlogService BlogSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<BlogPost>? _posts;
    private bool _expanded;
    private string _searchQuery = string.Empty;
    private string? _selectedTag;

    private IEnumerable<string> AllTags => _posts?
        .SelectMany(p => (p.Tags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
        .Select(t => t.Trim())
        .Where(t => !string.IsNullOrEmpty(t))
        .Distinct()
        .OrderBy(t => t)
        ?? Enumerable.Empty<string>();

    private bool IsFiltering => !string.IsNullOrWhiteSpace(_searchQuery) || _selectedTag is not null;

    private IEnumerable<BlogPost> FilteredPosts
    {
        get
        {
            if (_posts is null) return Enumerable.Empty<BlogPost>();
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

    private IEnumerable<BlogPost> VisiblePosts =>
        (_expanded || IsFiltering) ? FilteredPosts : FilteredPosts.Take(InitialCount);

    private bool HasMore => !IsFiltering && (_posts?.Count ?? 0) > InitialCount;

    protected override async Task OnInitializedAsync()
    {
        _posts = await BlogSvc.GetPublishedAsync();
        Lang.OnChange += StateHasChanged;
    }

    private void Toggle() => _expanded = !_expanded;

    private void SelectTag(string? tag)
    {
        _selectedTag = _selectedTag == tag ? null : tag;
        _expanded = false;
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
