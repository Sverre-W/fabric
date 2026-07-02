namespace Fabric.Server.Infrastructure.Tenancy;

public interface ITenantStore
{
    public Task<TenantInfo?> GetTenantAsync(string tenantId, CancellationToken cancellationToken);
    public Task<List<TenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken);
    public void InvalidateTenant(string tenantId);
}
