using Elsa.Common.Multitenancy;
using Fabric.Server.Infrastructure.Tenancy;

namespace Fabric.Server.Automation;


public class ElsaTenantResolver(ITenantContext tenantContext) : ITenantResolver
{
    public Task<TenantResolverResult> ResolveAsync(TenantResolverContext context)
    {
        string tenantId = tenantContext.TenantId;
        return Task.FromResult(TenantResolverResult.Resolved(tenantId));
    }
}
