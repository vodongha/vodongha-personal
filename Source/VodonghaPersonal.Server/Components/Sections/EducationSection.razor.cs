using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Components.Sections;

public partial class EducationSection : ComponentBase
{
    [Inject] private EducationService EduSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private const int InitialCount = 3;
    private List<Education>? _items;

    protected override async Task OnInitializedAsync()
    {
        _items = await EduSvc.GetAllAsync();
    }
}
