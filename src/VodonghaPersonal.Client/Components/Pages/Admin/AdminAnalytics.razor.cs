using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Shared.DTOs;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminAnalytics : ComponentBase, IDisposable
{
    [Inject] private AnalyticsApiClient AnalyticsClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _days = 30;
    private bool _loading = true;
    private bool _pendingChartRender = false;

    private int _total;
    private int _totalAll;
    private List<TopItemDto> _topPages = [];
    private List<TopItemDto> _topCountries = [];
    private List<TopItemDto> _topReferrers = [];
    private List<DailyViewDto> _daily = [];

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
        AnalyticsDto? data = await AnalyticsClient.GetAsync(_days);
        if (data != null)
        {
            _total = data.Total;
            _totalAll = data.TotalAll;
            _topPages = data.TopPages;
            _topCountries = data.TopCountries;
            _topReferrers = data.TopReferrers;
            _daily = data.Daily;
        }
        _loading = false;
    }

    private async Task RenderChartsAsync()
    {
        try
        {
            string[] dateLabels = _daily.Select(d => d.Date.ToString("MM/dd")).ToArray();
            int[] dailyCounts = _daily.Select(d => d.Count).ToArray();
            await JS.InvokeVoidAsync("analyticsCharts.renderLine", "chart-daily", dateLabels, dailyCounts);

            if (_topPages.Count > 0)
            {
                string[] pageLabels = _topPages.Select(p => p.Label).ToArray();
                int[] pageCounts = _topPages.Select(p => p.Count).ToArray();
                await JS.InvokeVoidAsync("analyticsCharts.renderBar", "chart-pages", pageLabels, pageCounts, "#22c9b7");
            }

            if (_topCountries.Count > 0)
            {
                string[] countryLabels = _topCountries.Select(c => c.Label).ToArray();
                int[] countryCounts = _topCountries.Select(c => c.Count).ToArray();
                await JS.InvokeVoidAsync("analyticsCharts.renderBar", "chart-countries", countryLabels, countryCounts, "#f59e0b");
            }
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (JSException ex) { Console.WriteLine($"Analytics chart JS error: {ex.Message}"); }
    }
}
