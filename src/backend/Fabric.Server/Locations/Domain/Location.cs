namespace Fabric.Server.Locations.Domain;

public abstract record Location
{
    private Location() { }

    public abstract Guid Id { get; }

    public sealed record SiteLocation(Site Site) : Location
    {
        public override Guid Id => Site.Id;
    }

    public sealed record BuildingLocation(Site Site, Building Building) : Location
    {
        public override Guid Id => Building.Id;
    }

    public sealed record RoomLocation(Site Site, Building Building, Room Room) : Location
    {
        public override Guid Id => Room.Id;
    }
}
