namespace Fabric.Server.Tenants.Domain;

public sealed record OidcSettings
{
    public string MetadataUrl { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public bool RequireHttpsMetadata { get; init; } = true;
}
