namespace Fabric.Server.Infrastructure.Tenancy;

public interface ITenantContextAccessor : ITenantContext
{
    void SetTenant(TenantInfo tenant);
}
