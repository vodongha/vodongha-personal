using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminHealth : ComponentBase, IAsyncDisposable
{
    [Inject] private HealthMonitorService Monitor { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IReadOnlyList<HealthMetricSnapshot> _snapshots = [];
    private HealthMetricSnapshot? _latest;
    private TimeSpan _uptime;
    private DateTime _startedAt;
    private bool _isRefreshing;
    private int _countdown = 30;

    // Table sort + pagination
    private string _sortCol = "Time";
    private bool _sortAsc = false;  // newest first by default
    private int _page = 0;
    private int _pageSize = 10;

    private IEnumerable<HealthMetricSnapshot> Sorted => _sortCol switch
    {
        "Memory"  => _sortAsc ? _snapshots.OrderBy(s => s.MemoryMb)  : _snapshots.OrderByDescending(s => s.MemoryMb),
        "DB Ping" => _sortAsc ? _snapshots.OrderBy(s => s.DbPingMs)  : _snapshots.OrderByDescending(s => s.DbPingMs),
        "Threads" => _sortAsc ? _snapshots.OrderBy(s => s.ThreadCount) : _snapshots.OrderByDescending(s => s.ThreadCount),
        "Status"  => _sortAsc ? _snapshots.OrderBy(s => s.DbHealthy)  : _snapshots.OrderByDescending(s => s.DbHealthy),
        _         => _sortAsc ? _snapshots.OrderBy(s => s.Timestamp)  : _snapshots.OrderByDescending(s => s.Timestamp),
    };

    private List<HealthMetricSnapshot> Paged => Sorted.Skip(_page * _pageSize).Take(_pageSize).ToList();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_snapshots.Count / (double)_pageSize));

    private void SortBy(string col)
    {
        if (_sortCol == col) { _sortAsc = !_sortAsc; }
        else { _sortCol = col; _sortAsc = col != "Time"; }
        _page = 0;
    }

    private string SortIcon(string col)
    {
        if (_sortCol != col) return "bi-chevron-expand";
        return _sortAsc ? "bi-chevron-up" : "bi-chevron-down";
    }

    private void GoPage(int p) => _page = Math.Clamp(p, 0, TotalPages - 1);

    private void OnPageSizeChange(ChangeEventArgs e)
    {
        _pageSize = int.TryParse(e.Value?.ToString(), out int v) ? v : 10;
        _page = 0;
    }

    private CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        Loc.OnChanged += OnLangChanged;
        _startedAt = Monitor.StartedAt;
        LoadData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitCharts();
            // Background loop: update every 30 seconds, countdown every second
            _ = RunRefreshLoopAsync(_cts.Token);
        }
    }

    private void LoadData()
    {
        _snapshots = Monitor.GetSnapshots();
        _latest = Monitor.Latest;
        _uptime = Monitor.Uptime;
    }

    private async Task InitCharts()
    {
        try
        {
            string[] labels = _snapshots.Select(s => s.Timestamp.ToLocalTime().ToString("HH:mm:ss")).ToArray();
            double[] memData = _snapshots.Select(s => (double)s.MemoryMb).ToArray();
            double[] dbData = _snapshots.Select(s => s.DbPingMs).ToArray();

            await JS.InvokeVoidAsync("healthChart.init",
                "chart-memory", labels, memData, "#6ee7b7", "Memory", "MB");
            await JS.InvokeVoidAsync("healthChart.init",
                "chart-db", labels, dbData, "#818cf8", "DB Ping", "ms");
        }
        catch { /* page might not be active */ }
    }

    private async Task UpdateCharts()
    {
        try
        {
            string[] labels = _snapshots.Select(s => s.Timestamp.ToLocalTime().ToString("HH:mm:ss")).ToArray();
            double[] memData = _snapshots.Select(s => (double)s.MemoryMb).ToArray();
            double[] dbData = _snapshots.Select(s => s.DbPingMs).ToArray();

            await JS.InvokeVoidAsync("healthChart.update", "chart-memory", labels, memData);
            await JS.InvokeVoidAsync("healthChart.update", "chart-db", labels, dbData);
        }
        catch { }
    }

    private async Task RunRefreshLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Count down 1 second at a time
            for (int i = 30; i > 0 && !ct.IsCancellationRequested; i--)
            {
                _countdown = i;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(1000, ct).ConfigureAwait(false);
            }

            if (ct.IsCancellationRequested) break;

            LoadData();
            await InvokeAsync(StateHasChanged);
            await UpdateCharts();
            _countdown = 30;
        }
    }

    private async Task Refresh()
    {
        _isRefreshing = true;
        StateHasChanged();
        await Task.Delay(300);
        LoadData();
        _isRefreshing = false;
        StateHasChanged();
        await UpdateCharts();
        _countdown = 30;
    }

    private static string FormatUptime(TimeSpan ts)
    {
        if (ts.TotalDays >= 1)
        {
            return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m";
        }

        if (ts.TotalHours >= 1)
        {
            return $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        }

        return $"{ts.Minutes}m {ts.Seconds}s";
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public async ValueTask DisposeAsync()
    {
        Loc.OnChanged -= OnLangChanged;
        await _cts.CancelAsync();
        _cts.Dispose();

        try
        {
            await JS.InvokeVoidAsync("healthChart.destroy", "chart-memory");
            await JS.InvokeVoidAsync("healthChart.destroy", "chart-db");
        }
        catch { }
    }
}
