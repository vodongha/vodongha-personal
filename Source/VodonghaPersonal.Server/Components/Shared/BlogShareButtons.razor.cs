using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VodonghaPersonal.Services;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Components.Shared;

public partial class BlogShareButtons : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired] public string Slug { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;

    private bool _copied;
    private string PostUrl => $"https://VodonghaPersonal.id.vn/blog/{Slug}";

    private async Task CopyLinkAsync()
    {
        try
        {
            await JS.InvokeVoidAsync("copyToClipboard", PostUrl);
            _copied = true;
            StateHasChanged();
            await Task.Delay(2000);
            _copied = false;
            StateHasChanged();
        }
        catch (JSDisconnectedException) { }
    }

    private async Task ShareLinkedInAsync()
    {
        string url = $"https://www.linkedin.com/sharing/share-offsite/?url={Uri.EscapeDataString(PostUrl)}";
        await JS.InvokeVoidAsync("open", url, "_blank", "width=600,height=500");
    }

    private async Task ShareXAsync()
    {
        string url = $"https://x.com/intent/tweet?url={Uri.EscapeDataString(PostUrl)}&text={Uri.EscapeDataString(Title)}";
        await JS.InvokeVoidAsync("open", url, "_blank", "width=600,height=400");
    }

    protected override void OnInitialized()
    {
        Lang.OnChange += OnLangChange;
    }

    private async void OnLangChange() => await InvokeAsync(StateHasChanged);

    public void Dispose() => Lang.OnChange -= OnLangChange;
}
