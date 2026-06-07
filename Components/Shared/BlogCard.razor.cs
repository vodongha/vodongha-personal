using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class BlogCard : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    [Parameter, EditorRequired] public BlogPost Item { get; set; } = default!;
    [Parameter] public EventCallback OnClick { get; set; }

    protected override void OnInitialized()
    {
        Tz.OnTimezoneSet += OnTimezoneUpdated;
    }

    private void OnTimezoneUpdated() => InvokeAsync(StateHasChanged);

    public void Dispose() => Tz.OnTimezoneSet -= OnTimezoneUpdated;
}
