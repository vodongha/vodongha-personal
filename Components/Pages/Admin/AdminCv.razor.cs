using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Data;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class AdminCv : ComponentBase, IDisposable
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;
    [Inject] private SiteSettingService SettingsSvc { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private CvData? _data;
    private bool _loading = true;  // only for initial data load, not for PDF (handled by API)
    private int _selectedTemplate = 0;

    protected override void OnInitialized()
    {
        Loc.OnChanged += OnLangChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        try
        {
            await LoadAsync();
        }
        catch (JSDisconnectedException) { /* user navigated away */ }
        catch (ObjectDisposedException) { /* component disposed */ }
        catch (OperationCanceledException) { /* cancelled */ }
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

    private void SelectTemplate(int t)
    {
        _selectedTemplate = t;
    }

    private async Task Download()
    {
        // Open PDF in a new tab — avoids navigating away from the current page
        // and bypasses the chrome-error context restriction on window.location.
        await JS.InvokeVoidAsync("open", $"/api/cv/download?template={_selectedTemplate}", "_blank");
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
