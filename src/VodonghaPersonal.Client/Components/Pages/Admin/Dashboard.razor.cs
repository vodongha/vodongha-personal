using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class Dashboard : ComponentBase, IDisposable
{
    [Inject] private DashboardApiClient DashClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private bool _loading = true;
    private bool _pendingChartRender;

    private int _totalVisitors;
    private int _pageViews30d;
    private int _totalBlogViews;
    private int _unreadMessages;
    private int _unreadChats;
    private int _skillCount;
    private int _projectCount;
    private int _blogCount;
    private int _expCount;
    private int _eduCount;

    private List<string> _skillCatLabels = [];
    private List<int> _skillCatData = [];
    private List<string> _blogLabels = [];
    private List<int> _blogData = [];
    private List<string> _trendLabels = [];
    private List<int> _trendData = [];
    private List<RecentContactDto> _recentContacts = [];

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        DashboardStatsDto? stats = await DashClient.GetStatsAsync();
        if (stats != null)
        {
            _totalVisitors = stats.TotalVisitors;
            _pageViews30d = stats.PageViews30d;
            _totalBlogViews = stats.TotalBlogViews;
            _unreadMessages = stats.UnreadMessages;
            _unreadChats = stats.UnreadChats;
            _skillCount = stats.SkillCount;
            _projectCount = stats.ProjectCount;
            _blogCount = stats.BlogCount;
            _expCount = stats.ExpCount;
            _eduCount = stats.EduCount;
            _skillCatLabels = stats.SkillCatLabels;
            _skillCatData = stats.SkillCatData;
            _blogLabels = stats.BlogLabels;
            _blogData = stats.BlogData;
            _trendLabels = stats.TrendLabels;
            _trendData = stats.TrendData;
            _recentContacts = stats.RecentContacts;
        }
        _loading = false;
        _pendingChartRender = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_pendingChartRender) { return; }
        _pendingChartRender = false;
        try
        {
            await JS.InvokeVoidAsync("dashboardCharts.renderDonut", "chart-skills-cat", _skillCatLabels, _skillCatData);
            await JS.InvokeVoidAsync("dashboardCharts.renderHBar", "chart-blog-views", _blogLabels, _blogData);
            await JS.InvokeVoidAsync("dashboardCharts.renderLine", "chart-trend", _trendLabels, _trendData);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() => Loc.OnChanged -= OnLangChanged;
}
