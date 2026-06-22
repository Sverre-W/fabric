using System.Text;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;

namespace AccessControl.Unipass.Filters;

public class AccessRuleFilter : IUnipassFilter<AccessRuleDto>
{
    private int? _id = null;

    public AccessRuleFilter WithId(int id)
    {
        _id = id;
        return this;
    }

    public string BuildQueryString()
    {
        StringBuilder filter = new StringBuilder();

        if (_id != null)
        {
            filter.Append($"filter=Id eq {_id}");
        }

        return filter.ToString();
    }
}
