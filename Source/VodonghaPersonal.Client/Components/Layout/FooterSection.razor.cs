using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Client.Components.Layout;

public partial class FooterSection : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private PublicSiteApiClient SiteClient { get; set; } = default!;

    private int _visitorCount;
    private Dictionary<string, string> _settings = new();

    private string S(string key) => _settings.TryGetValue(key, out string? v) ? v : "";

    protected override async Task OnInitializedAsync()
    {
        Lang.OnChange += StateHasChanged;
        _visitorCount = await SiteClient.GetVisitorCountAsync();
        _settings = await SiteClient.GetSettingsAsync();
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
