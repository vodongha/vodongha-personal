using Microsoft.AspNetCore.Components;
using vodongha.Services;

namespace vodongha.Components.Layout;

public partial class FooterSection : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private VisitorService VisitorSvc { get; set; } = default!;

    private int _visitorCount;

    protected override async Task OnInitializedAsync()
    {
        Lang.OnChange += StateHasChanged;
        _visitorCount = await VisitorSvc.GetCountAsync();
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
