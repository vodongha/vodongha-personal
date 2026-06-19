using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminDependencies : ComponentBase, IDisposable
{
    [Inject] private DependencyApiClient DepClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private bool _loading = true;
    private string? _error;
    private DateTime? _checkedAt;
    private List<DependencyDto> _all = [];
    private Dictionary<string, List<DependencyDto>> _groups = [];

    private int _outdatedCount;
    private int _okCount;
    private int _unknownCount;

    private string _search = "";
    private string? _statusFilter;
    private string? _typeFilter;

    private IEnumerable<KeyValuePair<string, List<DependencyDto>>> VisibleGroups =>
        _groups
            .Where(g => _typeFilter is null || g.Key == _typeFilter)
            .Select(g => new KeyValuePair<string, List<DependencyDto>>(
                g.Key,
                g.Value
                    .Where(d => _statusFilter is null || d.Status == _statusFilter)
                    .Where(d => string.IsNullOrWhiteSpace(_search) || d.Name.Contains(_search, StringComparison.OrdinalIgnoreCase))
                    .ToList()))
            .Where(g => g.Value.Count > 0);

    private int VisibleCount => VisibleGroups.Sum(g => g.Value.Count);

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _error = null;
        StateHasChanged();
        try
        {
            _all = await DepClient.GetAllAsync();
            _groups = _all
                .GroupBy(d => d.Type)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Status == "Outdated" ? 0 : d.Status == "Unknown" ? 1 : 2).ThenBy(d => d.Name).ToList());
            _outdatedCount = _all.Count(d => d.Status == "Outdated");
            _okCount = _all.Count(d => d.Status == "UpToDate");
            _unknownCount = _all.Count(d => d.Status == "Unknown");
            _checkedAt = DateTime.UtcNow;
        }
        catch (Exception ex) { _error = $"Failed to check dependencies: {ex.Message}"; }
        finally { _loading = false; }
    }

    private async Task Refresh()
    {
        await DepClient.InvalidateCacheAsync();
        await LoadAsync();
    }

    private void SetTypeFilter(string type) { _typeFilter = _typeFilter == type ? null : type; _statusFilter = null; }
    private void SetStatusFilter(string status) { _statusFilter = _statusFilter == status ? null : status; _typeFilter = null; }
    private void ClearFilters() { _typeFilter = null; _statusFilter = null; _search = ""; }

    private static string GroupIcon(string type) => type switch { "NuGet" => "bi-box-seam", "Npm" => "bi-npm", "Cdn" => "bi-cloud-download", "GitHubActions" => "bi-github", _ => "bi-box" };
    private static string StatusIcon(string status) => status switch { "UpToDate" => "bi-check-circle-fill", "Outdated" => "bi-exclamation-triangle-fill", _ => "bi-question-circle-fill" };

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() { Loc.OnChanged -= OnLangChanged; }
}
