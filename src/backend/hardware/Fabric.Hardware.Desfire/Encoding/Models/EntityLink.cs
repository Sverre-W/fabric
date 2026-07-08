namespace Fabric.Hardware.Desfire.Encoding.Models;

public record EntityLink
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;

    public EntityLink() { }

    public EntityLink(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
