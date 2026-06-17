namespace Fabric.Server.Reception.Domain;

public sealed class CheckInDocument
{
    private CheckInDocument() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public CheckInDocumentType DocumentType { get; private set; }
    public byte[] Content { get; private set; } = null!;

    internal static CheckInDocument Create(string name, CheckInDocumentType documentType, byte[] content) =>
        new() { Id = Guid.NewGuid(), Name = name, DocumentType = documentType, Content = content };
}