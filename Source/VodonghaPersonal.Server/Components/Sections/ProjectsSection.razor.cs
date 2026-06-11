using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class ProjectsSection : ComponentBase, IDisposable
{
    [Inject] private ProjectService ProjectSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<Project>? _projects;

    protected override async Task OnInitializedAsync()
    {
        _projects = await ProjectSvc.GetAllAsync();
        Lang.OnChange += StateHasChanged;
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
