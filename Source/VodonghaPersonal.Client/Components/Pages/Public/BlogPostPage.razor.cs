using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Client.Components.Pages.Public;

public partial class BlogPostPage : ComponentBase, IDisposable
{
    [Inject] private PublicBlogApiClient BlogClient { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public string Slug { get; set; } = string.Empty;

    private BlogPost? _post;
    private bool _loading = true;

    private string DisplayTitle => Lang.IsVi ? (_post?.Title ?? "") : (_post?.TitleEn ?? _post?.Title ?? "");
    private string DisplaySummary => Lang.IsVi ? (_post?.Summary ?? "") : (_post?.SummaryEn ?? _post?.Summary ?? "");
    private string DisplayContent => Lang.IsVi ? (_post?.Content ?? "") : (_post?.ContentEn ?? _post?.Content ?? "");

    private int ReadingMinutes
    {
        get
        {
            string? html = Lang.IsVi ? _post?.Content : (_post?.ContentEn ?? _post?.Content);
            if (string.IsNullOrEmpty(html))
            {
                return 1;
            }

            string text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
            int words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, (int)Math.Ceiling(words / 200.0));
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        Lang.OnChange -= StateHasChanged;
        _loading = true;
        _post = await BlogClient.GetBySlugAsync(Slug);
        _loading = false;
        Lang.OnChange += StateHasChanged;

        // Increment view count — fire and forget, don't block render
        if (_post is not null)
        {
            _ = BlogClient.IncrementViewAsync(_post.Id);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _post is null)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("initReadingProgress");
            await JS.InvokeVoidAsync("initCodeCopy",
                Lang.T("blog.code.copy"),
                Lang.T("blog.code.copied"));
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    // Note: TOC JS lifecycle (initToc / destroyToc) is managed by the
    // TableOfContents component itself via its own OnAfterRenderAsync / Dispose.

    protected override void OnInitialized()
    {
        Tz.OnTimezoneSet += OnTimezoneUpdated;
    }

    private async void OnTimezoneUpdated() => await InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Lang.OnChange -= StateHasChanged;
        Tz.OnTimezoneSet -= OnTimezoneUpdated;
        _ = JS.InvokeVoidAsync("destroyReadingProgress");
        _ = JS.InvokeVoidAsync("destroyCodeCopy");
    }
}
