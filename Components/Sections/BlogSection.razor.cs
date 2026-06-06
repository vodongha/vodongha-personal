using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Sections;

public partial class BlogSection : ComponentBase, IDisposable
{
    [Inject] private BlogService BlogSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<BlogPost>? _posts;
    private bool _expanded;

    private IEnumerable<BlogPost> VisiblePosts =>
        _expanded ? _posts! : _posts!.Take(InitialCount);

    protected override async Task OnInitializedAsync()
    {
        _posts = await BlogSvc.GetPublishedAsync();
        Lang.OnChange += StateHasChanged;
    }

    private void Toggle() => _expanded = !_expanded;

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
