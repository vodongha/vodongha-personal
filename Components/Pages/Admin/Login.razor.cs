using Microsoft.AspNetCore.Components;

namespace VodonghaPersonal.Components.Pages.Admin;

public partial class Login : ComponentBase
{
    [SupplyParameterFromQuery] private string? Error { get; set; }
    private bool ShowError => Error == "1";
}
