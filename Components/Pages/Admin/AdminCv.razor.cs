using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using vodongha.Data;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Pages.Admin;

public partial class AdminCv : ComponentBase, IDisposable
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private SiteSettingService SettingsSvc { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private CvData? _data;
    private bool _loading = true;  // only for initial data load, not for PDF (handled by API)

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

        Dictionary<string, string> settings = await SettingsSvc.GetAllAsync();

        await using AppDbContext db = await DbFactory.CreateDbContextAsync();
        List<Skill> skills = await db.Skills.OrderBy(s => s.Order).ToListAsync();
        List<Experience> experiences = await db.Experiences.OrderBy(e => e.Order).ToListAsync();
        List<Education> educations = await db.Educations.OrderBy(e => e.Order).ToListAsync();
        List<Project> projects = await db.Projects.OrderBy(p => p.Order).ToListAsync();

        _data = new CvData(
            Name:        settings.GetValueOrDefault("Name", ""),
            Title:       settings.GetValueOrDefault("Title", ""),
            Email:       settings.GetValueOrDefault("Email", ""),
            Phone:       settings.GetValueOrDefault("Phone", ""),
            Location:    settings.GetValueOrDefault("Location", ""),
            GitHub:      settings.GetValueOrDefault("GitHub", ""),
            LinkedIn:    settings.GetValueOrDefault("LinkedIn", ""),
            Bio:         settings.GetValueOrDefault("BioEn", settings.GetValueOrDefault("Bio", "")),
            AvatarUrl:   settings.GetValueOrDefault("AvatarUrl", ""),
            Skills:      skills,
            Experiences: experiences,
            Educations:  educations,
            Projects:    projects
        );

        _loading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void Download()
    {
        // Navigate to the API endpoint — PDF is streamed directly over HTTP,
        // avoiding the 32KB SignalR message size limit.
        Nav.NavigateTo("/api/cv/download", forceLoad: true);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string Initials(string name)
    {
        string[] parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
            : name.Length > 0 ? name[..Math.Min(2, name.Length)].ToUpper() : "?";
    }

    private static string DateRange(Experience exp)
    {
        string start = $"{MonthShort(exp.StartMonth)} {exp.StartYear}";
        string end = exp.IsCurrent ? "Present"
            : exp.EndYear.HasValue ? $"{MonthShort(exp.EndMonth ?? 1)} {exp.EndYear}" : "Present";
        return $"{start} – {end}";
    }

    private static string MonthShort(int month) => month switch
    {
        1 => "Jan", 2 => "Feb", 3 => "Mar", 4 => "Apr",
        5 => "May", 6 => "Jun", 7 => "Jul", 8 => "Aug",
        9 => "Sep", 10 => "Oct", 11 => "Nov", 12 => "Dec",
        _ => ""
    };

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() => Loc.OnChanged -= OnLangChanged;
}
