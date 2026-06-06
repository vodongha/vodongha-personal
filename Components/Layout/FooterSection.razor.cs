using Microsoft.AspNetCore.Components;
using vodongha.Services;

namespace vodongha.Components.Layout;

public partial class FooterSection : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private VisitorService VisitorSvc { get; set; } = default!;
    [Inject] private SiteSettingService SettingSvc { get; set; } = default!;

    private int _visitorCount;
    private Dictionary<string, string> _settings = new();

    private string S(string key) => _settings.TryGetValue(key, out string? v) ? v : "";

    protected override async Task OnInitializedAsync()
    {
        Lang.OnChange += StateHasChanged;
        _visitorCount = await VisitorSvc.GetCountAsync();
        _settings = await SettingSvc.GetAllAsync();
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
