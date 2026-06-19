namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContextAccessor
{
    public const string DefaultTenantId = "main-tenant";

    public string TenantId { get; private set; } = DefaultTenantId;

    public void SetTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));

        TenantId = tenantId.Trim();
    }
}
