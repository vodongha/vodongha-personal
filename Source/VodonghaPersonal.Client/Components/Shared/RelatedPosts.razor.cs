using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Client.Components.Shared;

public partial class RelatedPosts : ComponentBase, IDisposable
{
    [Inject] private PublicBlogApiClient BlogApi { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    [Parameter, EditorRequired] public int PostId { get; set; }
    [Parameter, EditorRequired] public string Tags { get; set; } = string.Empty;

    private List<BlogPost> _related = [];
    private bool _loading = true;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        _related = await BlogApi.GetRelatedAsync(PostId, Tags);
        _loading = false;
    }

    protected override void OnInitialized()
    {
        Lang.OnChange += OnLangChange;
        Tz.OnTimezoneSet += OnTimezoneUpdated;
    }

    private async void OnLangChange() => await InvokeAsync(StateHasChanged);
    private async void OnTimezoneUpdated() => await InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Lang.OnChange -= OnLangChange;
        Tz.OnTimezoneSet -= OnTimezoneUpdated;
    }
}
