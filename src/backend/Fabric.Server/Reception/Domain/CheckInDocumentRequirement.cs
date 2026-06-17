namespace Fabric.Server.Reception.Domain;

public sealed class CheckInDocumentRequirement
{
    public string Name { get; init; } = null!;
    public bool Required { get; init; }
    public CheckInDocumentType DocumentType { get; init; }
}