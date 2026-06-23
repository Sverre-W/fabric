using Microsoft.Extensions.Options;

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenantBaseUrlResolver(
    IOptions<TenancyOptions> options,
    ITenantContext tenantContext)
{
    public string GetBaseUrl()
    {
        string baseUrl = options.Value.TenantBaseUrl.Replace("{tenant}", tenantContext.TenantId, StringComparison.OrdinalIgnoreCase).TrimEnd('/');

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
            throw new InvalidOperationException("Tenancy:TenantBaseUrl must be an absolute URL.");

        return baseUrl;
    }
}
