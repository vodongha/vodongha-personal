using Microsoft.AspNetCore.Components.QuickGrid;

namespace VodonghaPersonal.Client.Components.Pages.Admin;

/// <summary>
/// Shared helpers for server-side admin QuickGrid pages.
/// </summary>
public static class AdminGrid
{
    /// <summary>
    /// Translate a QuickGrid sort request into the (sortBy, sortDir) pair the paged API expects.
    /// Returns (null, "asc") when no column sort is active.
    /// </summary>
    public static (string? SortBy, string SortDir) MapSort<T>(GridItemsProviderRequest<T> req)
    {
        IReadOnlyCollection<SortedProperty> sorts = req.GetSortByProperties();
        if (sorts.Count == 0) { return (null, "asc"); }
        SortedProperty sp = sorts.First();
        return (sp.PropertyName, sp.Direction == SortDirection.Descending ? "desc" : "asc");
    }

    /// <summary>Page index implied by a provider request (StartIndex / page size).</summary>
    public static int PageOf<T>(GridItemsProviderRequest<T> req, int fallbackPageSize)
    {
        int size = req.Count ?? fallbackPageSize;
        return size > 0 ? req.StartIndex / size : 0;
    }
}
