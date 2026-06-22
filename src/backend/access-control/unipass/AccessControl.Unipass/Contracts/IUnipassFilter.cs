using System.Text.Json.Serialization;
using AccessControl.Unipass.Filters;

namespace AccessControl.Unipass.Contracts;

/// <summary>
/// Represent a filter for a given Unipass entity
/// </summary>
public interface IUnipassFilter
{
    /// <summary>
    /// Returns a query string to be used to represent this filter
    /// </summary>
    /// <returns></returns>
    public string BuildQueryString();
}

public interface IUnipassFilter<T> : IUnipassFilter;
