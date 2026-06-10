using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class ExperienceSection : ComponentBase, IDisposable
{
    [Inject] private ExperienceService ExpSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<Experience>? _items;
    private bool _expanded;

    private IEnumerable<Experience> VisibleItems =>
        _expanded ? _items! : _items!.Take(InitialCount);

    protected override async Task OnInitializedAsync()
    {
        _items = await ExpSvc.GetAllAsync();
        Lang.OnChange += StateHasChanged;
    }

    private void Toggle() => _expanded = !_expanded;

    private static string MonthYear(int month, int year)
    {
        string[] months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        string m = month >= 1 && month <= 12 ? months[month - 1] : "";
        return $"{m} {year}";
    }

    public void Dispose() => Lang.OnChange -= StateHasChanged;
}
