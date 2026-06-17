namespace Fabric.Server.Locations.Domain;

public sealed class Room
{
    private Room() { }

    public Guid Id { get; internal set; }
    public string Name { get; internal set; } = null!;
    public int Capacity { get; internal set; }
    public bool WheelchairAccessible { get; internal set; }

    internal static Room Create(Guid id, string name, int capacity, bool wheelchairAccessible) =>
        new()
        {
            Id = id,
            Name = name,
            Capacity = capacity,
            WheelchairAccessible = wheelchairAccessible
        };

    internal void Update(string name, int capacity, bool wheelchairAccessible)
    {
        Name = name;
        Capacity = capacity;
        WheelchairAccessible = wheelchairAccessible;
    }
}
