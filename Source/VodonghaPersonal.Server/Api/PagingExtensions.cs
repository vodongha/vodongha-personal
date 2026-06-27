using Microsoft.EntityFrameworkCore;
using VodonghaPersonal.Shared.DTOs;

namespace VodonghaPersonal.Api;

/// <summary>
/// Server-side table processing helpers — turn an EF Core <see cref="IQueryable{T}"/>
/// into a single page of results plus the total count, so admin tables only ever
/// fetch the rows they display.
/// </summary>
public static class PagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize)
    {
        if (pageSize <= 0) { pageSize = 10; }
        if (pageSize > 200) { pageSize = 200; }
        if (page < 0) { page = 0; }

        int total = await query.CountAsync();
        List<T> items = await query.Skip(page * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T>(items, total);
    }
}
