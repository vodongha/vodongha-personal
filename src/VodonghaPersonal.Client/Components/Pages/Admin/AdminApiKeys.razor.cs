using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminApiKeys : ComponentBase, IDisposable
{
    [Inject] private ApiKeyApiClient ApiKeyClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

    private bool _loading = true;
    private bool _saving = false;

    private List<ApiKeyDefinitionDto> _definitions = [];
    private Dictionary<string, string> _dbValues = [];
    private Dictionary<string, string> _envValues = [];

    private string? _editingKey = null;
    private string _editValue = "";
    private string? _editError = null;
    private string _search = "";

    private IEnumerable<string> Categories => _definitions.Select(d => d.Category).Distinct();

    private IEnumerable<ApiKeyDefinitionDto> FilteredDefs(string category)
    {
        IEnumerable<ApiKeyDefinitionDto> defs = _definitions.Where(d => d.Category == category);
        if (string.IsNullOrWhiteSpace(_search)) { return defs; }
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
        if (firstRender) { await LoadAsync(); }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        await InvokeAsync(StateHasChanged);
        ApiKeysPageDto? data = await ApiKeyClient.GetAllAsync();
        if (data != null)
        {
            _definitions = data.Definitions;
            _dbValues = data.DbValues;
            _envValues = data.EnvValues;
        }
        _loading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void StartEdit(string key, string currentDbValue) { _editingKey = key; _editValue = currentDbValue; _editError = null; }
    private void CancelEdit() { _editingKey = null; _editValue = ""; _editError = null; }

    private async Task SaveEdit()
    {
        if (_editingKey == null) { return; }
        _saving = true;
        _editError = null;
        await InvokeAsync(StateHasChanged);

        try
        {
            await ApiKeyClient.SaveAsync(_editingKey, _editValue.Trim());
            Toast.Show(Loc.T("Saved successfully"), success: true);
            _editingKey = null;
            _editValue = "";
            await LoadAsync();
        }
        catch
        {
            _editError = Loc.T("Save failed. Check logs.");
        }
        finally
        {
            _saving = false;
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task RemoveOverride(string key)
    {
        await ApiKeyClient.DeleteAsync(key);
        Toast.Show(Loc.T("DB override removed"), success: true);
        await LoadAsync();
    }

    private static string MaskValue(string value, bool sensitive)
    {
        if (!sensitive || string.IsNullOrEmpty(value)) { return value; }
        if (value.Length <= 8) { return new string('•', value.Length); }
        return value[..4] + new string('•', Math.Min(value.Length - 8, 12)) + value[^4..];
    }

    private static string CategoryIcon(string category) => category switch
    {
        "Fly.io" => "bi-airplane",
        "Neon" => "bi-database",
        "Telegram" => "bi-telegram",
        "Email" => "bi-envelope",
        "Web Push" => "bi-bell",
        "Gemini" => "bi-stars",
        _ => "bi-key",
    };

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() => Loc.OnChanged -= OnLangChanged;
}
