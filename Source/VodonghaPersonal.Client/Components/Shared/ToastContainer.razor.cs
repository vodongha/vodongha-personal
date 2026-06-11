using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Client.Services;

namespace VodonghaPersonal.Client.Components.Shared;

public partial class ToastContainer : ComponentBase, IDisposable
{
    [Inject] private ToastService ToastSvc { get; set; } = default!;

    protected override void OnInitialized() => ToastSvc.OnChange += Refresh;

    private async void Refresh() => await InvokeAsync(StateHasChanged);

    public void Dispose() => ToastSvc.OnChange -= Refresh;
}
