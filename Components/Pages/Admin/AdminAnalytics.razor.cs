using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminAnalytics : ComponentBase
{
    [Inject] private AnalyticsService Analytics { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private int _days = 30;
    private bool _loading = true;

    private int _total;
    private int _totalAll;
    private List<(string Path, int Count)> _topPages = [];
    private List<(string Country, int Count)> _topCountries = [];
    private List<(string Referrer, int Count)> _topReferrers = [];
    private List<(DateTime Date, int Count)> _daily = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_loading)
        {
            await RenderChartsAsync();
        }
    }

    private async Task SetDays(int days)
    {
        _days = days;
        _loading = true;
        StateHasChanged();
        await LoadAsync();
        _loading = false;
        StateHasChanged();
        await RenderChartsAsync();
    }

    private async Task LoadAsync()
    {
        (_total, _totalAll, _topPages, _topCountries, _topReferrers, _daily) = await (
            Analytics.GetTotalAsync(_days),
            Analytics.GetTotalAsync(0),
            Analytics.GetTopPagesAsync(_days),
            Analytics.GetTopCountriesAsync(_days),
            Analytics.GetTopReferrersAsync(_days),
            Analytics.GetDailyViewsAsync(_days)
        ).WhenAll();

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
                string[] pageLabels = _topPages.Select(p => p.Path).ToArray();
                int[] pageCounts = _topPages.Select(p => p.Count).ToArray();
                await JS.InvokeVoidAsync("analyticsCharts.renderBar", "chart-pages", pageLabels, pageCounts);
            }

            if (_topCountries.Count > 0)
            {
                string[] countryLabels = _topCountries.Select(c => c.Country).ToArray();
                int[] countryCounts = _topCountries.Select(c => c.Count).ToArray();
                await JS.InvokeVoidAsync("analyticsCharts.renderBar", "chart-countries", countryLabels, countryCounts);
            }
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }
}

file static class TaskExtensions
{
    public static async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(
        this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>) tasks)
    {
        await Task.WhenAll(tasks.Item1, tasks.Item2, tasks.Item3, tasks.Item4, tasks.Item5, tasks.Item6);
        return (tasks.Item1.Result, tasks.Item2.Result, tasks.Item3.Result,
                tasks.Item4.Result, tasks.Item5.Result, tasks.Item6.Result);
    }
}
