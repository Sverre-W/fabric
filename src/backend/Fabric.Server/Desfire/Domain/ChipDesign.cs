namespace Fabric.Server.Desfire.Domain;

public sealed class ChipDesign
{
    private ChipDesign() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int Version { get; private set; }
    public string? Description { get; private set; }
    public string SpecificationJson { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    public static ChipDesign Create(string name, int version, string? description, string specificationJson, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        Version = version,
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        SpecificationJson = specificationJson,
        CreatedAt = now
    };

    public void Update(string name, int version, string? description, string specificationJson)
    {
        Name = name.Trim();
        Version = version;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SpecificationJson = specificationJson;
    }
}
