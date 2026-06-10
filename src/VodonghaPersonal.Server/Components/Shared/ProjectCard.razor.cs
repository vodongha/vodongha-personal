using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Shared;

public partial class ProjectCard : ComponentBase
{
    [Inject] private LanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired] public Project Item { get; set; } = default!;
}
