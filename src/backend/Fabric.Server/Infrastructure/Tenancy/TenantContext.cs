using Fabric.Server.Tenants.Domain;

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenantContext : ITenantContextAccessor
{
    public const string DefaultTenantId = "main-tenant";

    public string TenantId { get; private set; } = DefaultTenantId;
    public TenantConfiguration Configuration { get; private set; } = new()
    {
        Oidc = new OidcSettings
        {
            MetadataUrl = "http://localhost/.well-known/openid-configuration",
            ClientId = "fabric",
            RequireHttpsMetadata = false
        }
    };

    public void SetTenant(TenantInfo tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.Id))
            throw new ArgumentException("Tenant id is required.", nameof(tenant));

        TenantId = tenant.Id.Trim();
        Configuration = tenant.Configuration;
    }
}
