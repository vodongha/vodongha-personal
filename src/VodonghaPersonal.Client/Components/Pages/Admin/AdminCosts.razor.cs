using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminCosts : ComponentBase, IDisposable
{
    [Inject] private CostApiClient CostClient { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;
    [Inject] private ApiKeyApiClient ApiKeyClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private CostSummaryDto? _summary;
    private bool _loading = true;

    private string _flyOrder = "[]";
    private string _neonOrder = "[]";

    private DotNetObjectReference<AdminCosts>? _dotNetRef;

    private const string FlyPrefKey = "_pref.costs.fly";
    private const string NeonPrefKey = "_pref.costs.neon";

    private double TotalEstimated =>
        (_summary?.Fly?.EstimatedBillable ?? 0) + (_summary?.Neon?.EstimatedMonthlyCost ?? 0);

    protected override void OnInitialized()
    {
        Loc.OnChanged += OnLangChanged;
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }
        try
        {
            _flyOrder = await ApiKeyClient.GetValueAsync(FlyPrefKey) ?? "[]";
            _neonOrder = await ApiKeyClient.GetValueAsync(NeonPrefKey) ?? "[]";
            await LoadAsync();
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        await InvokeAsync(StateHasChanged);
        _summary = await CostClient.GetAsync();
        _loading = false;
        await InvokeAsync(StateHasChanged);
        try
        {
            await JS.InvokeVoidAsync("initSortableCards", "fly-cards-grid", _dotNetRef, FlyPrefKey);
            await JS.InvokeVoidAsync("initSortableCards", "neon-cards-grid", _dotNetRef, NeonPrefKey);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    private async Task Refresh()
    {
        await CostClient.InvalidateCacheAsync();
        await LoadAsync();
    }

    [JSInvokable]
    public async Task SaveCardOrder(string prefKey, string[] ids)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(ids);
        await ApiKeyClient.SaveAsync(prefKey, json);
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    private static string FormatUsd(double value) => $"${value:F2}";
    private static string MachineCardClass(string state) => state switch { "started" => "health-card--ok", "suspended" => "", _ => "health-card--error" };
    private static string MachineIcon(string state) => state switch { "started" => "bi-play-circle-fill", "suspended" => "bi-pause-circle", _ => "bi-stop-circle" };
    private string MachinStateLabel(string state) => state switch { "started" => Loc.T("Running"), "suspended" => Loc.T("Suspended"), "stopped" => Loc.T("Stopped"), _ => state };

    public void Dispose() { Loc.OnChanged -= OnLangChanged; _dotNetRef?.Dispose(); }
}
