using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Sections;

public partial class EducationSection : ComponentBase, IDisposable
{
    [Inject] private EducationService EduSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<Education>? _items;
    private bool _expanded;

    private IEnumerable<Education> VisibleItems =>
        _expanded ? _items! : _items!.Take(InitialCount);

    protected override async Task OnInitializedAsync()
    {
        _items = await EduSvc.GetAllAsync();
        Lang.OnChange += StateHasChanged;
    }

    private void Toggle() => _expanded = !_expanded;

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
