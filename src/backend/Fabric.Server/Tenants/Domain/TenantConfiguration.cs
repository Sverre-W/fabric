namespace Fabric.Server.Tenants.Domain;

public sealed record TenantConfiguration
{
    public OidcSettings Oidc { get; init; } = null!;
}
