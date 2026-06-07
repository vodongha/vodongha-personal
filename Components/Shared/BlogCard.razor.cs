using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class BlogCard : ComponentBase
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    [Parameter, EditorRequired] public BlogPost Item { get; set; } = default!;
    [Parameter] public EventCallback OnClick { get; set; }
}
