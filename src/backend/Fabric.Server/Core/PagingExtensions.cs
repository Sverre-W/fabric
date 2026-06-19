using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Core;

public static class PagingExtensions
{
    public static async Task<Page<T>> GetPageAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        int totalCount = await query.CountAsync(cancellationToken);

        List<T> currentPage = await query
            .Take(pageSize)
            .Skip(pageNumber * pageSize)
            .ToListAsync(cancellationToken);

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new Page<T>()
        {
            CurrentPage = pageNumber,
            IsLastPage = (pageNumber + 1) == totalPages,
            Items = currentPage,
            PageSize = pageSize,
            TotalItems = totalCount
        };
    }
}
