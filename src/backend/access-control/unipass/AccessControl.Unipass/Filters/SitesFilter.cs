using System.Text;
using System.Text.Json.Serialization;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;

namespace AccessControl.Unipass.Filters;

public class SitesFilter : IUnipassFilter<UnipassSite>
{
    [JsonInclude]
    public string? Name { get; private set; }

    public SitesFilter WithName(string name)
    {
        Name = name;
        return this;
    }

    public string BuildQueryString()
    {
        StringBuilder filter = new StringBuilder();

        if (Name != null)
        {
            filter.Append($"filter=Name1 eq {Name} or Name2 eq {Name} or Name3 eq {Name}");
        }

        return filter.ToString();
    }
}
