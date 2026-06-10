using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class AdminApiKeys : ComponentBase, IDisposable
{
    [Inject] private AppSecretsService Secrets { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;
    [Inject] private IConfiguration Config { get; set; } = default!;

    private bool _loading = true;
    private bool _saving  = false;

    // key → decrypted DB value (or empty if no DB override)
    private Dictionary<string, string> _dbValues  = [];
    // key → env/config value
    private Dictionary<string, string> _envValues = [];

    private string? _editingKey  = null;
    private string  _editValue   = "";
    private string? _editError   = null;
    private string  _search      = "";

    private static readonly string[] _categories =
        AppSecretsService.Definitions.Select(d => d.Category).Distinct().ToArray();

    private IEnumerable<AppSecretDefinition> FilteredDefs(string category)
    {
        IEnumerable<AppSecretDefinition> defs = AppSecretsService.Definitions.Where(d => d.Category == category);
        if (string.IsNullOrWhiteSpace(_search))
        {
            return defs;
        }

        string q = _search.Trim();
        return defs.Where(d =>
            d.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            d.Key.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            d.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
    }

    private bool CategoryVisible(string category) => FilteredDefs(category).Any();

    protected override void OnInitialized() => Loc.OnChanged += OnLangChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        await InvokeAsync(StateHasChanged);

        // Load DB overrides
        List<VodonghaPersonal.Data.Models.AppSecret> dbSecrets = await Secrets.GetAllAsync();
        _dbValues = dbSecrets.ToDictionary(s => s.Key, s => s.Value);

        // Load env values for all defined keys
        _envValues = [];
        foreach (AppSecretDefinition def in AppSecretsService.Definitions)
        {
            string? envVal = Config[def.Key];
            if (!string.IsNullOrEmpty(envVal))
            {
                _envValues[def.Key] = envVal;
            }
        }

        _loading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void StartEdit(string key, string currentDbValue)
    {
        _editingKey = key;
        _editValue  = currentDbValue; // pre-fill with DB value if exists
        _editError  = null;
    }

    private void CancelEdit()
    {
        _editingKey = null;
        _editValue  = "";
        _editError  = null;
    }

    private async Task SaveEdit()
    {
        if (_editingKey == null)
        {
            return;
        }

        _saving    = true;
        _editError = null;
        await InvokeAsync(StateHasChanged);

        bool ok = await Secrets.SaveAsync(_editingKey, _editValue.Trim());

        _saving = false;

        if (ok)
        {
            Toast.Show(Loc.T("Saved successfully"), success: true);
            _editingKey = null;
            _editValue  = "";
            await LoadAsync();
        }
        else
        {
            _editError = Loc.T("Save failed. Check logs.");
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveOverride(string key)
    {
        bool ok = await Secrets.SaveAsync(key, ""); // empty = remove
        if (ok)
        {
            Toast.Show(Loc.T("DB override removed"), success: true);
            await LoadAsync();
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string MaskValue(string value, bool sensitive)
    {
        if (!sensitive || string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (value.Length <= 8)
        {
            return new string('•', value.Length);
        }

        return value[..4] + new string('•', Math.Min(value.Length - 8, 12)) + value[^4..];
    }

    private static string CategoryIcon(string category) => category switch
    {
        "Fly.io"    => "bi-airplane",
        "Neon"      => "bi-database",
        "Telegram"  => "bi-telegram",
        "Email"     => "bi-envelope",
        "Web Push"  => "bi-bell",
        "Gemini"    => "bi-stars",
        _           => "bi-key",
    };

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public void Dispose() => Loc.OnChanged -= OnLangChanged;
}
