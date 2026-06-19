namespace Fabric.Server.Core;

/// <summary>
///     Represent a paged result. Pages start at 0
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPaged<T>
{
    /// <summary>
    ///     The current page.
    /// </summary>
    public int CurrentPage { get; }

    /// <summary>
    ///     The total amount of pages (calculated via <see cref="TotalItems" /> and <see cref="PageSize" />
    ///     )
    ///     Set to null if PageSize is = 0 or if TotalItems is = null
    /// </summary>
    public int? TotalPages { get; }

    /// <summary>
    ///     The amount of items per page
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    ///     The total amount of items. Null if not available. Use <see cref="IsLastPage" /> to determine
    ///     if there are more pages
    /// </summary>
    public int? TotalItems { get; }

    /// <summary>
    ///     The items on the page
    /// </summary>
    public IList<T> Items { get; }

    /// <summary>
    ///     Indicates if this page is the last page.
    /// </summary>
    public bool IsLastPage { get; }

    public Page<TTarget> Map<TTarget>(Func<T, TTarget> func)
    {
        return new Page<TTarget>
        {
            CurrentPage = CurrentPage,
            PageSize = PageSize,
            TotalItems = TotalItems,
            IsLastPage = IsLastPage,
            Items = Items.Select(func).ToList(),
        };
    }
}

public class Page<T> : IPaged<T>
{
    public int CurrentPage { get; set; }

    public int? TotalPages => 0 == PageSize || TotalItems == null ? null : (TotalItems + PageSize - 1) / PageSize;

    public int PageSize { get; set; }
    public int? TotalItems { get; set; }
    public IList<T> Items { get; set; } = new List<T>();
    public bool IsLastPage { get; set; }
}


public interface IPageable
{
    /// <summary>
    ///     The current page
    /// </summary>
    public int Page { get; }

    /// <summary>
    ///     The amount of items per page
    /// </summary>
    public int PageSize { get; }
}

public class Pageable : IPageable
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 25;
}
