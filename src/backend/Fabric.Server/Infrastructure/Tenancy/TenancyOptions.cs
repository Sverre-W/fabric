using Fabric.Server.Notifications;

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenancyOptions
{
    public const string SectionName = "Tenancy";

    public TenancyMode Mode { get; set; } = TenancyMode.SingleTenant;
    public DefaultTenantOptions DefaultTenant { get; set; } = new();
}

public sealed class DefaultTenantOptions
{
    public string Id { get; set; } = TenantContext.DefaultTenantId;
    public OidcOptions Oidc { get; set; } = new();
    public GraphEmailSettings? GraphEmail { get; set; }
}

public class OidcOptions
{
    public string? MetadataUrl { get; set; }
    public string? ClientId { get; set; }
    public bool RequireHttpsMetadata { get; set; } = true;

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(MetadataUrl)
        && !string.IsNullOrWhiteSpace(ClientId);
}
