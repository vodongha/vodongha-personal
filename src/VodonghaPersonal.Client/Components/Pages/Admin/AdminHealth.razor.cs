using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminHealth : ComponentBase, IAsyncDisposable
{
    [Inject] private HealthApiClient HealthClient { get; set; } = default!;
    [Inject] private ApiKeyApiClient ApiKeyClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private List<HealthSnapshotDto> _snapshots = [];
    private HealthSnapshotDto? _latest;
    private long _uptimeSeconds;
    private DateTime _startedAt;
    private bool _isRefreshing;
    private int _countdown = 30;

    private string _statOrder = "[]";
    private string _chartOrder = "[]";
    private DotNetObjectReference<AdminHealth>? _dotNetRef;

    private const string StatPrefKey = "_pref.health.stats";
    private const string ChartPrefKey = "_pref.health.charts";

    private string _sortCol = "Time";
    private bool _sortAsc = false;
    private int _page = 0;
    private int _pageSize = 10;

    private IEnumerable<HealthSnapshotDto> Sorted => _sortCol switch
    {
        "Memory" => _sortAsc ? _snapshots.OrderBy(s => s.MemoryMb) : _snapshots.OrderByDescending(s => s.MemoryMb),
        "DB Ping" => _sortAsc ? _snapshots.OrderBy(s => s.DbPingMs) : _snapshots.OrderByDescending(s => s.DbPingMs),
        "Threads" => _sortAsc ? _snapshots.OrderBy(s => s.ThreadCount) : _snapshots.OrderByDescending(s => s.ThreadCount),
        "Status" => _sortAsc ? _snapshots.OrderBy(s => s.DbHealthy) : _snapshots.OrderByDescending(s => s.DbHealthy),
        _ => _sortAsc ? _snapshots.OrderBy(s => s.Timestamp) : _snapshots.OrderByDescending(s => s.Timestamp),
    };

    private List<HealthSnapshotDto> Paged => Sorted.Skip(_page * _pageSize).Take(_pageSize).ToList();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_snapshots.Count / (double)_pageSize));

    private void SortBy(string col) { if (_sortCol == col) { _sortAsc = !_sortAsc; } else { _sortCol = col; _sortAsc = col != "Time"; } _page = 0; }
    private string SortIcon(string col) { if (_sortCol != col) { return "↕"; } return _sortAsc ? "↑" : "↓"; }
    private void GoPage(int p) => _page = Math.Clamp(p, 0, TotalPages - 1);
    private void OnPageSizeChange(ChangeEventArgs e) { _pageSize = int.TryParse(e.Value?.ToString(), out int v) ? v : 10; _page = 0; }

    private CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        Loc.OnChanged += OnLangChanged;
        Tz.OnTimezoneSet += OnTimezoneUpdated;
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    private void OnTimezoneUpdated() => InvokeAsync(StateHasChanged);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }
        try
        {
            _statOrder = await ApiKeyClient.GetValueAsync(StatPrefKey) ?? "[]";
            _chartOrder = await ApiKeyClient.GetValueAsync(ChartPrefKey) ?? "[]";
            await LoadDataAsync();
            await InvokeAsync(StateHasChanged);
            await InitCharts();
            await JS.InvokeVoidAsync("initSortableCards", "health-stat-cards", _dotNetRef, StatPrefKey);
            await JS.InvokeVoidAsync("initSortableCards", "health-chart-cards", _dotNetRef, ChartPrefKey);
            _ = RunRefreshLoopAsync(_cts.Token);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
    }

    [JSInvokable]
    public async Task SaveCardOrder(string prefKey, string[] ids)
    {
        string json = System.Text.Json.JsonSerializer.Serialize(ids);
        await ApiKeyClient.SaveAsync(prefKey, json);
    }

    private async Task LoadDataAsync()
    {
        HealthDataDto? data = await HealthClient.GetAsync();
        if (data != null)
        {
            _snapshots = data.Snapshots;
            _latest = data.Latest;
            _uptimeSeconds = data.UptimeSeconds;
            _startedAt = data.StartedAt;
        }
    }

    private async Task InitCharts()
    {
        try
        {
            string[] labels = _snapshots.Select(s => Tz.ToUserTime(s.Timestamp).ToString("HH:mm:ss")).ToArray();
            double[] memData = _snapshots.Select(s => (double)s.MemoryMb).ToArray();
            double[] dbData = _snapshots.Select(s => s.DbPingMs).ToArray();
            await JS.InvokeVoidAsync("healthChart.init", "chart-memory", labels, memData, "#6ee7b7", "Memory", "MB");
            await JS.InvokeVoidAsync("healthChart.init", "chart-db", labels, dbData, "#818cf8", "DB Ping", "ms");
        }
        catch { }
    }

    private async Task UpdateCharts()
    {
        try
        {
            string[] labels = _snapshots.Select(s => Tz.ToUserTime(s.Timestamp).ToString("HH:mm:ss")).ToArray();
            double[] memData = _snapshots.Select(s => (double)s.MemoryMb).ToArray();
            double[] dbData = _snapshots.Select(s => s.DbPingMs).ToArray();
            await JS.InvokeVoidAsync("healthChart.update", "chart-memory", labels, memData);
            await JS.InvokeVoidAsync("healthChart.update", "chart-db", labels, dbData);
        }
        catch { }
    }

    private async Task RunRefreshLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                for (int i = 30; i > 0 && !ct.IsCancellationRequested; i--)
                {
                    _countdown = i;
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(1000, ct).ConfigureAwait(false);
                }
                if (ct.IsCancellationRequested) { break; }
                await LoadDataAsync();
                await InvokeAsync(StateHasChanged);
                await UpdateCharts();
                _countdown = 30;
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (JSDisconnectedException) { }
    }

    private async Task Refresh()
    {
        _isRefreshing = true;
        StateHasChanged();
        await Task.Delay(300);
        await LoadDataAsync();
        _isRefreshing = false;
        StateHasChanged();
        await UpdateCharts();
        _countdown = 30;
    }

    private static string FormatUptime(long seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        if (ts.TotalDays >= 1) { return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m"; }
        if (ts.TotalHours >= 1) { return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s"; }
        return $"{ts.Minutes}m {ts.Seconds}s";
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public async ValueTask DisposeAsync()
    {
        Loc.OnChanged -= OnLangChanged;
        Tz.OnTimezoneSet -= OnTimezoneUpdated;
        _dotNetRef?.Dispose();
        await _cts.CancelAsync();
        _cts.Dispose();
        try { await JS.InvokeVoidAsync("healthChart.destroy", "chart-memory"); await JS.InvokeVoidAsync("healthChart.destroy", "chart-db"); } catch { }
    }
}
