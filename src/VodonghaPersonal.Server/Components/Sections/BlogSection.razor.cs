using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class BlogSection : ComponentBase, IDisposable
{
    [Inject] private BlogService BlogSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int PreviewCount = 3;
    private List<BlogPost>? _posts;

    protected override async Task OnInitializedAsync()
    {
        _posts = await BlogSvc.GetPublishedAsync();
        Lang.OnChange += StateHasChanged;
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
