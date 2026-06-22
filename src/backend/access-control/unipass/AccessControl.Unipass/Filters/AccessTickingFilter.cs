using System.Text;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;

namespace AccessControl.Unipass.Filters;
public class AccessTickingFilter : IUnipassFilter<AccessTicking>
{
    private int? _visitorId = null;

    public AccessTickingFilter WithVisitorId(int id)
    {
        _visitorId = id;
        return this;
    }

    public string BuildQueryString()
    {
        StringBuilder filter = new StringBuilder();

        if (_visitorId != null)
        {
            filter.Append($"filter=Person eq {_visitorId}");
        }

        return filter.ToString();
    }
}
