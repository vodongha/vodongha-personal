using System.Reflection;
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
    private string _appVersion = "";

    private string S(string key) => _settings.TryGetValue(key, out string? v) ? v : "";

    protected override async Task OnInitializedAsync()
    {
        Lang.OnChange += StateHasChanged;
        _visitorCount = await SiteClient.GetVisitorCountAsync();
        _settings = await SiteClient.GetSettingsAsync();
        _appVersion = ResolveAppVersion();
    }

    private static string ResolveAppVersion()
    {
        string? assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3);
        if (assemblyVersion is not null && assemblyVersion != "0.0.0" && assemblyVersion != "1.0.0")
        {
            return assemblyVersion;
        }
        return Environment.GetEnvironmentVariable("APP_VERSION") ?? "";
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
