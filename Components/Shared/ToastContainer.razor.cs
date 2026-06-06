using Microsoft.AspNetCore.Components;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class ToastContainer : ComponentBase, IDisposable
{
    [Inject] private ToastService ToastSvc { get; set; } = default!;

    protected override void OnInitialized() => ToastSvc.OnChange += Refresh;

    private void Refresh() => InvokeAsync(StateHasChanged);

    public void Dispose() => ToastSvc.OnChange -= Refresh;
}
