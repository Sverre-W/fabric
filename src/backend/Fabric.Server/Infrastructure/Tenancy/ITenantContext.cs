using Fabric.Server.Tenants.Domain;

namespace Fabric.Server.Infrastructure.Tenancy;

public interface ITenantContext
{
    string TenantId { get; }
    TenantConfiguration Configuration { get; }
}
