using Microsoft.AspNetCore.Components;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminCosts : ComponentBase, IDisposable
{
    [Inject] private CostMonitorService CostMonitor { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;
    [Inject] private ToastService Toast { get; set; } = default!;

    private CostSummary? _summary;
    private bool _loading = true;
    private string? _restartingId;   // machine ID currently being restarted
    private bool _confirmRestart;
    private string? _pendingRestartId;

    private double TotalEstimated =>
        (_summary?.Fly?.EstimatedBillable ?? 0) +
        (_summary?.Neon?.EstimatedMonthlyCost ?? 0);

    protected override void OnInitialized()
    {
        Loc.OnChanged += OnLangChanged;
    }

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
        _summary = await CostMonitor.GetSummaryAsync();
        _loading = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task Refresh()
    {
        CostMonitor.InvalidateCache();
        await LoadAsync();
    }

    // ─── Restart ─────────────────────────────────────────────────────────────

    private async Task RestartMachine(string machineId)
    {
        _pendingRestartId = machineId;
        _confirmRestart = true;
        await InvokeAsync(StateHasChanged);
    }

    private void CancelRestart()
    {
        _confirmRestart = false;
        _pendingRestartId = null;
    }

    private async Task ConfirmRestart()
    {
        if (_pendingRestartId == null)
        {
            return;
        }

        _confirmRestart = false;
        _restartingId = _pendingRestartId;
        _pendingRestartId = null;
        await InvokeAsync(StateHasChanged);

        bool ok = await CostMonitor.RestartMachineAsync(_restartingId);

        _restartingId = null;

        if (ok)
        {
            Toast.Show(Loc.T("Machine restarted successfully"), success: true);
            // Wait a moment for Fly.io to update state, then reload
            await Task.Delay(3000);
            await LoadAsync();
        }
        else
        {
            Toast.Show(Loc.T("Failed to restart machine. Check Fly:ApiToken."), success: false);
            await InvokeAsync(StateHasChanged);
        }
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
    }
}
