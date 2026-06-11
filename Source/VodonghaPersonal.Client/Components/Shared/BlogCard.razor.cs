using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Shared.Models;
using VodonghaPersonal.Shared.Services;

namespace VodonghaPersonal.Client.Components.Shared;

public partial class BlogCard : ComponentBase, IDisposable
{
    [Inject] private LanguageService Lang { get; set; } = default!;
    [Inject] private TimezoneService Tz { get; set; } = default!;

    [Parameter, EditorRequired] public BlogPost Item { get; set; } = default!;
    [Parameter] public EventCallback OnClick { get; set; }

    private int ReadingMinutes
    {
        get
        {
            string? html = Lang.IsVi ? Item.Content : (Item.ContentEn ?? Item.Content);
            if (string.IsNullOrEmpty(html))
            {
                return 1;
            }

            string text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
            int words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return Math.Max(1, (int)Math.Ceiling(words / 200.0));
        }
    }

    protected override void OnInitialized()
    {
        Tz.OnTimezoneSet += OnTimezoneUpdated;
    }

    private async void OnTimezoneUpdated() => await InvokeAsync(StateHasChanged);

    public void Dispose() => Tz.OnTimezoneSet -= OnTimezoneUpdated;
}
