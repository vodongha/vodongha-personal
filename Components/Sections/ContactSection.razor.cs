using Microsoft.AspNetCore.Components;
using vodongha.Data.Models;
using vodongha.Services;

namespace vodongha.Components.Sections;

public partial class ContactSection : ComponentBase, IDisposable
{
    [Inject] private ContactService ContactSvc { get; set; } = default!;
    [Inject] private LanguageService Lang { get; set; } = default!;

    private ContactMessage _model = new();
    private bool _sending;
    private bool _sent;

    protected override void OnInitialized() => Lang.OnChange += StateHasChanged;

    public void Dispose() => Lang.OnChange -= StateHasChanged;

    private async Task HandleSubmit()
    {
        _sending = true;
        await ContactSvc.SendAsync(_model);
        _sent = true;
        _sending = false;
    }
}
