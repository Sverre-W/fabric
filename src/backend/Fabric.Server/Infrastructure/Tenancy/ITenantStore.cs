namespace Fabric.Server.Infrastructure.Tenancy;

public interface ITenantStore
{
    Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken);
}
