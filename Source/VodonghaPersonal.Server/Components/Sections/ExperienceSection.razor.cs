using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class ExperienceSection : ComponentBase
{
    [Inject] private ExperienceService ExpSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<Experience>? _items;

    protected override async Task OnInitializedAsync()
    {
        _items = await ExpSvc.GetAllAsync();
    }

    private static string MonthYear(int month, int year)
    {
        string[] months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        string m = month >= 1 && month <= 12 ? months[month - 1] : "";
        return $"{m} {year}";
    }
}
