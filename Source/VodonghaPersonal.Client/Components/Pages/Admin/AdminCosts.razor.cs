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

    private string _neonOrder = "[]";

    private DotNetObjectReference<AdminCosts>? _dotNetRef;

    private const string NeonPrefKey = "_pref.costs.neon";

    private double TotalEstimated => _summary?.Neon?.EstimatedMonthlyCost ?? 0;

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

    public void Dispose() { Loc.OnChanged -= OnLangChanged; _dotNetRef?.Dispose(); }
}
