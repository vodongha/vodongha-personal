using Microsoft.AspNetCore.Components;
using VodonghaPersonal.Services;

namespace VodonghaPersonal.Components.Shared;

public partial class Pagination : ComponentBase
{
    [Inject] private LanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired] public int CurrentPage { get; set; }
    [Parameter, EditorRequired] public int TotalPages { get; set; }
    [Parameter, EditorRequired] public EventCallback<int> OnPageChange { get; set; }

    /// <summary>
    /// Builds a compact page number list with ellipsis for large page counts.
    /// e.g. [1] … [4] [5*] [6] … [12]
    /// </summary>
    private IEnumerable<int> PageNumbers
    {
        get
        {
            if (TotalPages <= 7)
            {
                return Enumerable.Range(1, TotalPages);
            }

            List<int> pages = [];
            const int wing = 1; // pages shown each side of current

            pages.Add(1);

            if (CurrentPage - wing > 2)
            {
                pages.Add(-1); // ellipsis
            }

            for (int i = Math.Max(2, CurrentPage - wing); i <= Math.Min(TotalPages - 1, CurrentPage + wing); i++)
            {
                pages.Add(i);
            }

            if (CurrentPage + wing < TotalPages - 1)
            {
                pages.Add(-1); // ellipsis
            }

            pages.Add(TotalPages);

            return pages;
        }
    }
}
