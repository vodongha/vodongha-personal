using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminAnalytics : ComponentBase, IDisposable
{
    [Inject] private AnalyticsService Analytics { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _days = 30;
    private bool _loading = true;
    private bool _pendingChartRender = false;

    private int _total;
    private int _totalAll;
    private List<(string Path, int Count)> _topPages = [];
    private List<(string Country, int Count)> _topCountries = [];
    private List<(string Referrer, int Count)> _topReferrers = [];
    private List<(DateTime Date, int Count)> _daily = [];

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        await LoadAsync();
        _pendingChartRender = true;
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public void Dispose() => Loc.OnChanged -= OnLangChanged;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingChartRender)
        {
            _pendingChartRender = false;
            await RenderChartsAsync();
        }
    }

    private async Task SetDays(int days)
    {
        _days = days;
        _loading = true;
        await InvokeAsync(StateHasChanged);

        await LoadAsync();

        _pendingChartRender = true;
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadAsync()
    {
        List<Task> tasks =
        [
            LoadTotalAsync(),
            LoadTotalAllAsync(),
            LoadTopPagesAsync(),
            LoadTopCountriesAsync(),
            LoadTopReferrersAsync(),
            LoadDailyAsync(),
        ];
        await Task.WhenAll(tasks);
        _loading = false;
    }

    private async Task LoadTotalAsync()          { _total         = await Analytics.GetTotalAsync(_days); }
    private async Task LoadTotalAllAsync()       { _totalAll      = await Analytics.GetTotalAsync(0); }
    private async Task LoadTopPagesAsync()       { _topPages      = await Analytics.GetTopPagesAsync(_days); }
    private async Task LoadTopCountriesAsync()   { _topCountries  = await Analytics.GetTopCountriesAsync(_days); }
    private async Task LoadTopReferrersAsync()   { _topReferrers  = await Analytics.GetTopReferrersAsync(_days); }
    private async Task LoadDailyAsync()          { _daily         = await Analytics.GetDailyViewsAsync(_days); }

    private async Task RenderChartsAsync()
    {
        try
        {
            string[] dateLabels  = _daily.Select(d => d.Date.ToString("MM/dd")).ToArray();
            int[]    dailyCounts = _daily.Select(d => d.Count).ToArray();
            await JS.InvokeVoidAsync("analyticsCharts.renderLine", "chart-daily", dateLabels, dailyCounts);

            if (_topPages.Count > 0)
            {
                string[] pageLabels = _topPages.Select(p => p.Path).ToArray();
                int[]    pageCounts = _topPages.Select(p => p.Count).ToArray();
                await JS.InvokeVoidAsync("analyticsCharts.renderBar", "chart-pages", pageLabels, pageCounts);
            }

            if (_topCountries.Count > 0)
            {
                string[] countryLabels = _topCountries.Select(c => c.Country).ToArray();
                int[]    countryCounts = _topCountries.Select(c => c.Count).ToArray();
                await JS.InvokeVoidAsync("analyticsCharts.renderBar", "chart-countries", countryLabels, countryCounts);
            }
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (JSException ex)
        {
            Console.WriteLine($"Analytics chart JS error: {ex.Message}");
        }
    }
}
