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
    private Dictionary<DependencyType, List<DependencyInfo>> _groups = [];

    private int _outdatedCount;
    private int _okCount;
    private int _unknownCount;

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _error   = null;
        StateHasChanged();

        try
        {
            var all = await DepCheck.GetAllAsync();
            _groups = all
                .GroupBy(d => d.Type)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.Status).ThenBy(d => d.Name).ToList());

            _outdatedCount = all.Count(d => d.Status == DependencyStatus.Outdated);
            _okCount       = all.Count(d => d.Status == DependencyStatus.UpToDate);
            _unknownCount  = all.Count(d => d.Status == DependencyStatus.Unknown);
            _checkedAt     = DateTime.UtcNow;
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

    private static string GroupIcon(DependencyType type) => type switch
    {
        DependencyType.NuGet => "bi-box-seam",
        DependencyType.Npm   => "bi-npm",
        DependencyType.Cdn   => "bi-cloud-download",
        _                    => "bi-box"
    };

    private static string StatusIcon(DependencyStatus status) => status switch
    {
        DependencyStatus.UpToDate => "bi-check-circle-fill",
        DependencyStatus.Outdated => "bi-exclamation-triangle-fill",
        _                         => "bi-question-circle-fill"
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
