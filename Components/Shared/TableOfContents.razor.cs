using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using vodongha.Services;

namespace vodongha.Components.Shared;

public partial class TableOfContents : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter, EditorRequired] public string Content { get; set; } = string.Empty;

    private record TocItem(string Id, string Text, int Level);

    private List<TocItem> _items = [];
    private string _activeId = string.Empty;
    private bool _show;

    protected override void OnParametersSet()
    {
        _items = ExtractHeadings(Content);
        _show = _items.Count >= 4;
    }

    private static List<TocItem> ExtractHeadings(string html)
    {
        List<TocItem> items = [];
        MatchCollection matches = Regex.Matches(html, @"<h([23])[^>]*>(.*?)</h\1>", RegexOptions.IgnoreCase);
        foreach (Match m in matches)
        {
            int level = int.Parse(m.Groups[1].Value);
            string text = Regex.Replace(m.Groups[2].Value, "<[^>]+>", "").Trim();
            string id = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9\s-]", "")
                             .Trim()
                             .Replace(' ', '-');
            if (!string.IsNullOrEmpty(text))
            {
                items.Add(new TocItem(id, text, level));
            }
        }

        return items;
    }

    [JSInvokable]
    public void SetActiveHeading(string id)
    {
        if (_activeId != id)
        {
            _activeId = id;
            InvokeAsync(StateHasChanged);
        }
    }

    private DotNetObjectReference<TableOfContents>? _dotnetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !_show) return;
        _dotnetRef = DotNetObjectReference.Create(this);
        try
        {
            await JS.InvokeVoidAsync("initToc", _dotnetRef,
                _items.Select(i => i.Id).ToArray());
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    protected override void OnInitialized()
    {
        Lang.OnChange += OnLangChange;
    }

    private void OnLangChange() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Lang.OnChange -= OnLangChange;
        _dotnetRef?.Dispose();
        _ = JS.InvokeVoidAsync("destroyToc");
    }
}
