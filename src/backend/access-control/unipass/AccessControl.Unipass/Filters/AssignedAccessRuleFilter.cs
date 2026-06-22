using System.Text;
using System.Text.Json.Serialization;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;

namespace AccessControl.Unipass.Filters;

public class AssignedAccessRuleFilter : IUnipassFilter<UnipassAssignedAccessRule>
{
    [JsonInclude]
    public int? PersonId { get; private set; }

    public AssignedAccessRuleFilter WithPersonId(int id)
    {
        PersonId = id;
        return this;
    }

    public string BuildQueryString()
    {
        StringBuilder filter = new StringBuilder();

        if (PersonId != null)
        {
            filter.Append($"filter=Person eq {PersonId}");
        }

        return filter.ToString();
    }
}
