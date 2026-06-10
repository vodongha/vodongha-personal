using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Client.ApiClients;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

public partial class AdminCv : ComponentBase, IDisposable
{
    [Inject] private SettingsApiClient SettingsClient { get; set; } = default!;
    [Inject] private SkillApiClient SkillClient { get; set; } = default!;
    [Inject] private ExperienceApiClient ExpClient { get; set; } = default!;
    [Inject] private EducationApiClient EduClient { get; set; } = default!;
    [Inject] private ProjectApiClient ProjectClient { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private AdminLocalizationService Loc { get; set; } = default!;

    private CvData? _data;
    private bool _loading = true;
    private int _selectedTemplate = 0;

    protected override void OnInitialized() { Loc.OnChanged += OnLangChanged; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) { return; }
        try { await LoadAsync(); }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
        catch (OperationCanceledException) { }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        await InvokeAsync(StateHasChanged);

        Dictionary<string, string> settings = await SettingsClient.GetAllAsync();
        List<Skill> skills = await SkillClient.GetAllAsync();
        List<Experience> experiences = await ExpClient.GetAllAsync();
        List<Education> educations = await EduClient.GetAllAsync();
        List<Project> projects = await ProjectClient.GetAllAsync();

        _data = new CvData(
            Name: settings.GetValueOrDefault("Name", ""),
            Title: settings.GetValueOrDefault("Title", ""),
            Email: settings.GetValueOrDefault("Email", ""),
            Phone: settings.GetValueOrDefault("Phone", ""),
            Location: settings.GetValueOrDefault("Location", ""),
            GitHub: settings.GetValueOrDefault("GitHub", ""),
            LinkedIn: settings.GetValueOrDefault("LinkedIn", ""),
            Bio: settings.GetValueOrDefault("BioEn", settings.GetValueOrDefault("Bio", "")),
            AvatarUrl: settings.GetValueOrDefault("AvatarUrl", ""),
            Skills: skills, Experiences: experiences, Educations: educations, Projects: projects
        );
        _loading = false;
        await InvokeAsync(StateHasChanged);
    }

    private void SelectTemplate(int t) { _selectedTemplate = t; }

    private async Task Download()
    {
        await JS.InvokeVoidAsync("open", $"/api/cv/download?template={_selectedTemplate}", "_blank");
    }

    private static string Initials(string name)
    {
        string[] parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? $"{parts[0][0]}{parts[^1][0]}".ToUpper() : name.Length > 0 ? name[..Math.Min(2, name.Length)].ToUpper() : "?";
    }

    private string DateRange(Experience exp)
    {
        string present = Loc.T("Present");
        string start = $"{MonthShort(exp.StartMonth)} {exp.StartYear}";
        string end = exp.IsCurrent ? present : exp.EndYear.HasValue ? $"{MonthShort(exp.EndMonth ?? 1)} {exp.EndYear}" : present;
        return $"{start} – {end}";
    }

    private static string MonthShort(int month) => month switch { 1 => "Jan", 2 => "Feb", 3 => "Mar", 4 => "Apr", 5 => "May", 6 => "Jun", 7 => "Jul", 8 => "Aug", 9 => "Sep", 10 => "Oct", 11 => "Nov", 12 => "Dec", _ => "" };

    private async Task OnLangChanged() => await InvokeAsync(StateHasChanged);
    public void Dispose() => Loc.OnChanged -= OnLangChanged;
}
