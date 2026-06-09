using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Data.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class ProjectsSection : ComponentBase, IDisposable
{
    [Inject] private ProjectService ProjectSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<Project>? _projects;
    private bool _expanded;

    private IEnumerable<Project> VisibleProjects =>
        _expanded ? _projects! : _projects!.Take(InitialCount);

    protected override async Task OnInitializedAsync()
    {
        _projects = await ProjectSvc.GetAllAsync();
        Lang.OnChange += StateHasChanged;
    }

    private void Toggle() => _expanded = !_expanded;

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
