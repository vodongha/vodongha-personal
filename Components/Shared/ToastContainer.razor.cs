using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Shared;

public partial class ToastContainer : ComponentBase, IDisposable
{
    [Inject] private ToastService ToastSvc { get; set; } = default!;

    protected override void OnInitialized() => ToastSvc.OnChange += Refresh;

    private void Refresh() => InvokeAsync(StateHasChanged);

    public void Dispose() => ToastSvc.OnChange -= Refresh;
}
