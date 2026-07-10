using System.Reflection;
using Elsa.Workflows.UIHints.Dropdown;
using Fabric.Server.Desfire.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Automation.Kiosk.Providers;

public class EncodingTransformationProvider(DesfireDbContext desfireDbContext) : DropDownOptionsProviderBase
{
    protected override bool RefreshOnChange { get; } = true;

    protected override async ValueTask<ICollection<SelectListItem>> GetItemsAsync(PropertyInfo propertyInfo, object? context, CancellationToken cancellationToken)
    {
        if (context is null)
            return []; //Asked outside of tenant scope, return empty list

        List<SelectListItem> items = await desfireDbContext.Transformations
            .Select(x => new SelectListItem( x.Name, x.Id.ToString()))
            .ToListAsync(cancellationToken);

        return items;
    }
}


