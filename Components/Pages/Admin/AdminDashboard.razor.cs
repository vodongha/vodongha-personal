using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using VodonghaPersonal.Data;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class AdminDashboard : ComponentBase, IDisposable
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private ChatService ChatSvc { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private bool _loading = true;
    private bool _pendingChartRender;

    // Stat card values
    private int _totalVisitors;
    private int _pageViews30d;
    private int _totalBlogViews;
    private int _unreadMessages;
    private int _unreadChats;

    // Content counts
    private int _skillCount;
    private int _projectCount;
    private int _blogCount;
    private int _expCount;
    private int _eduCount;

    // Chart data
    private List<string> _skillCatLabels = [];
    private List<int>    _skillCatData   = [];
    private List<string> _blogLabels     = [];
    private List<int>    _blogData       = [];
    private List<string> _trendLabels    = [];
    private List<int>    _trendData      = [];

    // Recent contacts
    private List<RecentContact> _recentContacts = [];

    protected override async Task OnInitializedAsync()
    {
        Loc.OnChanged += OnLangChanged;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        DateTime now      = DateTime.UtcNow;
        DateTime cutoff30 = now.AddDays(-30);
        DateTime cutoff14 = now.AddDays(-14);

        await using AppDbContext db = await DbFactory.CreateDbContextAsync();

        // Stat cards
        _totalVisitors   = await db.VisitorLogs.CountAsync();
        _pageViews30d    = await db.PageViews.CountAsync(p => p.CreatedAt >= cutoff30);
        _totalBlogViews  = await db.BlogPosts.SumAsync(b => (int?)b.ViewCount) ?? 0;
        _unreadMessages  = await db.ContactMessages.CountAsync(m => !m.IsRead);
        _unreadChats     = await ChatSvc.GetUnreadCountAsync();

        // Content counts
        _skillCount   = await db.Skills.CountAsync();
        _projectCount = await db.Projects.CountAsync();
        _blogCount    = await db.BlogPosts.CountAsync();
        _expCount     = await db.Experiences.CountAsync();
        _eduCount     = await db.Educations.CountAsync();

        // Skills by category (donut)
        List<CategoryCount> skillsByCategory = await db.Skills
            .GroupBy(s => s.Category)
            .Select(g => new CategoryCount { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        _skillCatLabels = skillsByCategory.Select(x => x.Label).ToList();
        _skillCatData   = skillsByCategory.Select(x => x.Count).ToList();

        // Blog posts by view count (horizontal bar, top 6)
        List<CategoryCount> blogViews = await db.BlogPosts
            .OrderByDescending(b => b.ViewCount)
            .Take(6)
            .Select(b => new CategoryCount
            {
                Label = b.TitleEn != null ? b.TitleEn : b.Title,
                Count = b.ViewCount
            })
            .ToListAsync();
        _blogLabels = blogViews.Select(x => x.Label).ToList();
        _blogData   = blogViews.Select(x => x.Count).ToList();

        // 14-day page view trend
        List<DateCount> trend = await db.PageViews
            .Where(p => p.CreatedAt >= cutoff14)
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new DateCount { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        // Fill missing days with 0
        for (int i = 0; i < 14; i++)
        {
            DateTime day = cutoff14.Date.AddDays(i);
            DateCount? found = trend.FirstOrDefault(t => t.Date == day);
            _trendLabels.Add(day.ToString("MM/dd"));
            _trendData.Add(found?.Count ?? 0);
        }

        // Recent contacts (last 5)
        _recentContacts = await db.ContactMessages
            .OrderByDescending(m => m.SentAt)
            .Take(5)
            .Select(m => new RecentContact
            {
                Name   = m.Name,
                Message = m.Message.Length > 60 ? m.Message.Substring(0, 58) + "…" : m.Message,
                SentAt = m.SentAt,
                IsRead = m.IsRead
            })
            .ToListAsync();

        _loading = false;
        _pendingChartRender = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_pendingChartRender) return;
        _pendingChartRender = false;
        try
        {
            await JS.InvokeVoidAsync("dashboardCharts.renderDonut", "chart-skills-cat",
                _skillCatLabels, _skillCatData);
            await JS.InvokeVoidAsync("dashboardCharts.renderHBar", "chart-blog-views",
                _blogLabels, _blogData);
            await JS.InvokeVoidAsync("dashboardCharts.renderLine", "chart-trend",
                _trendLabels, _trendData);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException)  { }
    }

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);

    public void Dispose() => Loc.OnChanged -= OnLangChanged;

    // Local DTOs
    private sealed record CategoryCount { public string Label { get; init; } = ""; public int Count { get; init; } }
    private sealed record DateCount     { public DateTime Date { get; init; }  public int Count { get; init; } }
    public  sealed record RecentContact
    {
        public string   Name      { get; init; } = "";
        public string   Message   { get; init; } = "";
        public DateTime SentAt { get; init; }
        public bool     IsRead    { get; init; }
    }
}
