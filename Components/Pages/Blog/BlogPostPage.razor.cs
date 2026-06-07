using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Blog;

public partial class BlogPostPage : ComponentBase, IDisposable
{
    [Inject] private BlogService BlogSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = string.Empty;

    private BlogPost? _post;

    private string DisplayTitle => Lang.IsVi ? (_post?.Title ?? "") : (_post?.TitleEn ?? _post?.Title ?? "");
    private string DisplaySummary => Lang.IsVi ? (_post?.Summary ?? "") : (_post?.SummaryEn ?? _post?.Summary ?? "");
    private string DisplayContent => Lang.IsVi ? (_post?.Content ?? "") : (_post?.ContentEn ?? _post?.Content ?? "");

    protected override async Task OnParametersSetAsync()
    {
        Lang.OnChange -= StateHasChanged;
        _post = await BlogSvc.GetBySlugAsync(Slug);
        Lang.OnChange += StateHasChanged;
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
