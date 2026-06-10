using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class AdminDependencies : ComponentBase
{
    [Inject] private DependencyCheckService DepCheck { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private bool _loading = true;
    private string? _error;
    private DateTime? _checkedAt;
    private IReadOnlyList<DependencyInfo> _all = [];
    private Dictionary<DependencyType, List<DependencyInfo>> _groups = [];

    private int _outdatedCount;
    private int _okCount;
    private int _unknownCount;

    private string _search = "";
    private DependencyStatus? _statusFilter;
    private DependencyType? _typeFilter;

    private IEnumerable<KeyValuePair<DependencyType, List<DependencyInfo>>> VisibleGroups =>
        _groups
            .Where(g => _typeFilter is null || g.Key == _typeFilter)
            .Select(g => new KeyValuePair<DependencyType, List<DependencyInfo>>(
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
            _all = await DepCheck.GetAllAsync();
            _groups = _all
                .GroupBy(d => d.Type)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Status == DependencyStatus.Outdated ? 0 : d.Status == DependencyStatus.Unknown ? 1 : 2).ThenBy(d => d.Name).ToList());

            _outdatedCount = _all.Count(d => d.Status == DependencyStatus.Outdated);
            _okCount = _all.Count(d => d.Status == DependencyStatus.UpToDate);
            _unknownCount = _all.Count(d => d.Status == DependencyStatus.Unknown);
            _checkedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _error = $"Failed to check dependencies: {ex.Message}";
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task Refresh()
    {
        DepCheck.InvalidateCache();
        await LoadAsync();
    }

    private void SetTypeFilter(DependencyType type)
    {
        _typeFilter = _typeFilter == type ? null : type;
        _statusFilter = null;
    }

    private void SetStatusFilter(DependencyStatus status)
    {
        _statusFilter = _statusFilter == status ? null : status;
        _typeFilter = null;
    }

    private void ClearFilters()
    {
        _typeFilter = null;
        _statusFilter = null;
        _search = "";
    }

    private static string GroupIcon(DependencyType type) => type switch
    {
        DependencyType.NuGet => "bi-box-seam",
        DependencyType.Npm => "bi-npm",
        DependencyType.Cdn => "bi-cloud-download",
        _ => "bi-box"
    };

    private static string StatusIcon(DependencyStatus status) => status switch
    {
        DependencyStatus.UpToDate => "bi-check-circle-fill",
        DependencyStatus.Outdated => "bi-exclamation-triangle-fill",
        _ => "bi-question-circle-fill"
    };

    private async Task OnLangChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Loc.OnChanged -= OnLangChanged;
    }
}
