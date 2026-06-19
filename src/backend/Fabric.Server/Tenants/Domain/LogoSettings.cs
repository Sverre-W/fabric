namespace Fabric.Server.Tenants.Domain;

public sealed record LogoSettings
{
    public const int MaxDataLength = 1_048_576;

    public string ContentType { get; init; } = null!;
    public byte[] Data { get; init; } = [];
}
