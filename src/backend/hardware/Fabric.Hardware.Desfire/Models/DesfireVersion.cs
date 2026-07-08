namespace Fabric.Hardware.Desfire.Models;

public class DesfireVersion
{
    public VersionInfo? Hardware { get; init; }
    public VersionInfo? Software { get; init; }
    public required string CardId { get; init; }
}
