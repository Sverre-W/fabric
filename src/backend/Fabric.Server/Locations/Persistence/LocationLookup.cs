namespace Fabric.Server.Locations.Persistence;

public sealed class LocationLookup
{
    private LocationLookup() { }

    public Guid Id { get; private set; }
    public LocationType Type { get; private set; }
    public Guid SiteId { get; private set; }
    public Guid? BuildingId { get; private set; }
    public Guid? RoomId { get; private set; }

    public static LocationLookup Site(Guid siteId) =>
        new()
        {
            Id = siteId,
            Type = LocationType.Site,
            SiteId = siteId
        };

    public static LocationLookup Building(Guid siteId, Guid buildingId) =>
        new()
        {
            Id = buildingId,
            Type = LocationType.Building,
            SiteId = siteId,
            BuildingId = buildingId
        };

    public static LocationLookup Room(Guid siteId, Guid buildingId, Guid roomId) =>
        new()
        {
            Id = roomId,
            Type = LocationType.Room,
            SiteId = siteId,
            BuildingId = buildingId,
            RoomId = roomId
        };
}
