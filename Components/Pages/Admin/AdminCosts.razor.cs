using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminCosts : ComponentBase, IDisposable
{
    [Inject] private CostMonitorService CostMonitor { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;
    [Inject] private AppSecretsService Secrets { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private CostSummary? _summary;
    private bool _loading = true;

    // Saved card orders (JSON arrays) — passed as data-saved-order to the grid
    private string _flyOrder  = "[]";
    private string _neonOrder = "[]";

    private DotNetObjectReference<AdminCosts>? _dotNetRef;

    private const string FlyPrefKey  = "_pref.costs.fly";
    private const string NeonPrefKey = "_pref.costs.neon";

    private double TotalEstimated =>
        (_summary?.Fly?.EstimatedBillable ?? 0) +
        (_summary?.Neon?.EstimatedMonthlyCost ?? 0);

    protected override void OnInitialized()
    {
        Loc.OnChanged += OnLangChanged;
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load saved orders before loading data so grids render with correct order
            _flyOrder  = await Secrets.GetValueAsync(FlyPrefKey)  ?? "[]";
            _neonOrder = await Secrets.GetValueAsync(NeonPrefKey) ?? "[]";

            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        await InvokeAsync(StateHasChanged);
        _summary = await CostMonitor.GetSummaryAsync();
        _loading = false;
        await InvokeAsync(StateHasChanged);

        // Init SortableJS after cards are rendered
        await JS.InvokeVoidAsync("initSortableCards", "fly-cards-grid",  _dotNetRef, FlyPrefKey);
        await JS.InvokeVoidAsync("initSortableCards", "neon-cards-grid", _dotNetRef, NeonPrefKey);
    }

    private async Task Refresh()
    {
        CostMonitor.InvalidateCache();
        await LoadAsync();
    }

    /// <summary>Called by JS when user finishes dragging a card.</summary>
    [JSInvokable]
    public async Task SaveCardOrder(string prefKey, string[] ids)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(ids);
        await Secrets.SaveAsync(prefKey, json);
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string FormatUsd(double value) => $"${value:F2}";

    private static string MachineCardClass(string state) => state switch
    {
        "started"   => "health-card--ok",
        "suspended" => "",
        _           => "health-card--error"
    };

    private static string MachineIcon(string state) => state switch
    {
        "started"   => "bi-play-circle-fill",
        "suspended" => "bi-pause-circle",
        _           => "bi-stop-circle"
    };

    private string MachinStateLabel(string state) => state switch
    {
        "started"   => Loc.T("Running"),
        "suspended" => Loc.T("Suspended"),
        "stopped"   => Loc.T("Stopped"),
        _           => state
    };

    public void Dispose()
    {
        Loc.OnChanged -= OnLangChanged;
        _dotNetRef?.Dispose();
    }
}
