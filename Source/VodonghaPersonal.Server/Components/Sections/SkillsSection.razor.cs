using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class SkillsSection : ComponentBase, IDisposable
{
    [Inject] private SkillService SkillSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private Dictionary<string, List<Skill>>? _grouped;

    protected override async Task OnInitializedAsync()
    {
        _grouped = await SkillSvc.GetGroupedAsync();
        Lang.OnChange += StateHasChanged;
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
