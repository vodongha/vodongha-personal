using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Sections;

public partial class SkillsSection : ComponentBase, IDisposable
{
    [Inject] private SkillService SkillSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private Dictionary<string, List<Skill>>? _grouped;
    private bool _expanded;

    private IEnumerable<KeyValuePair<string, List<Skill>>> VisibleCategories =>
        _expanded ? _grouped! : _grouped!.Take(InitialCount);

    protected override async Task OnInitializedAsync()
    {
        _grouped = await SkillSvc.GetGroupedAsync();
        Lang.OnChange += StateHasChanged;
    }

    private void Toggle() => _expanded = !_expanded;

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
