using System.Text;
using System.Text.Json.Serialization;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;

namespace AccessControl.Unipass.Filters;

public class PersonFilter : IUnipassFilter<UnipassPerson>
{
    [JsonInclude]
    public bool? OnlyWithCards { get; private set; }

    [JsonInclude]
    public int? Id { get; private set; }

    [JsonInclude]
    public bool? ActiveOnly { get; private set; }

    [JsonInclude]
    public UnipassPersonType? AccessType { get; private set; }

    public PersonFilter WithCards(bool withCards = true)
    {
        OnlyWithCards = withCards;
        return this;
    }

    public PersonFilter AreEnabled(bool? activeOnly = true)
    {
        ActiveOnly = activeOnly;
        return this;
    }

    public PersonFilter WithAccessType(UnipassPersonType? accessType)
    {
        AccessType = accessType;
        return this;
    }

    public PersonFilter WithId(int id)
    {
        Id = id;
        return this;
    }

    public string BuildQueryString()
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder personFilter = new StringBuilder();

        if (AccessType != null)
        {
            personFilter.Append($"AccessType eq {(int)AccessType}");
        }

        if (ActiveOnly.HasValue)
        {
            string bit = ActiveOnly.Value ? "1" : "0";
            personFilter.Append($" and PersonEnabled eq '{bit}'");
        }

        if (OnlyWithCards ?? false)
        {
            if (sb.Length > 0)
                sb.Append(" and");

            sb.Append(" (Badge1 ne 0 or Badge2 ne 0 or Badge3 ne 0 or Badge4 ne 0 or Badge5 ne 0)");
        }

        if (Id.HasValue)
        {
            if (personFilter.Length > 0)
                personFilter.Append(" and");

            personFilter.Append($"Person eq {Id}");
        }

        if (personFilter.Length > 0)
        {
            if (sb.Length > 0)
                sb.Append(" and");

            sb.Append($"personFilter=({personFilter.ToString()})");
        }

        return sb.ToString();
    }
}
