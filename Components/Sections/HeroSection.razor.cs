using Microsoft.AspNetCore.Components;
using vodongha.Services;

namespace vodongha.Components.Sections;

public partial class HeroSection : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private SiteSettingService SettingSvc { get; set; } = default!;

    private Dictionary<string, string> _settings = new();
    private bool _loaded = false;

    private string Bio => Lang.IsVi ? S("Bio") : (S("BioEn") is { Length: > 0 } en ? en : S("Bio"));
    private string S(string key) => _settings.TryGetValue(key, out string? v) ? v : "";

    protected override async Task OnInitializedAsync()
    {
        _settings = await SettingSvc.GetAllAsync();
        _loaded = true;
        Lang.OnChange += StateHasChanged;
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
