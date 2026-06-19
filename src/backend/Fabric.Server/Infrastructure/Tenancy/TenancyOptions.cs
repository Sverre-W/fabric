namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenancyOptions
{
    public const string SectionName = "Tenancy";

    public TenancyMode Mode { get; init; } = TenancyMode.SingleTenant;
    public DefaultTenantOptions DefaultTenant { get; init; } = new();
}

public sealed class DefaultTenantOptions
{
    public string Id { get; init; } = TenantContext.DefaultTenantId;
    public OidcOptions Oidc { get; init; } = new();
}

public class OidcOptions
{
    public string? MetadataUrl { get; init; }
    public string? ClientId { get; init; }
    public bool RequireHttpsMetadata { get; init; } = true;

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(MetadataUrl)
        && !string.IsNullOrWhiteSpace(ClientId);
}
