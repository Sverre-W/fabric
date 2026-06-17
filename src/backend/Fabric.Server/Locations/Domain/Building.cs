namespace Fabric.Server.Locations.Domain;

public sealed class Building
{
    private Building() { }

    public Guid Id { get; internal set; }
    public string Name { get; internal set; } = null!;
    public string? Address { get; internal set; }
    public List<Room> Rooms { get; private set; } = [];

    internal static Building Create(Guid id, string name, string? address) =>
        new()
        {
            Id = id,
            Name = name,
            Address = address,
            Rooms = []
        };

    internal void AddRoom(Room room) => Rooms.Add(room);

    internal void RemoveRoom(Room room) => Rooms.Remove(room);

    internal void Rename(string name) => Name = name;
}
