using Fabric.Server.Locations.Domain;
using Fabric.Server.Locations.Persistence;

namespace Fabric.Server.Locations.Contracts;

public record SiteResponse(
    Guid Id,
    string Name,
    string? Address
);

public record BuildingResponse(
    Guid Id,
    string Name,
    string? Address
);

public record RoomResponse(
    Guid Id,
    string Name,
    int Capacity,
    bool WheelchairAccessible
);

public record LocationResponse(
    Guid Id,
    LocationType Type,
    SiteResponse Site,
    BuildingResponse? Building,
    RoomResponse? Room
);

public static class LocationMapper
{
    public static SiteResponse ToResponse(this Site site) =>
        new(site.Id, site.Name, site.Address);

    public static BuildingResponse ToResponse(this Building building) =>
        new(building.Id, building.Name, building.Address);

    public static RoomResponse ToResponse(this Room room) =>
        new(room.Id, room.Name, room.Capacity, room.WheelchairAccessible);

    public static LocationResponse ToResponse(this Location location) =>
        location switch
        {
            Location.SiteLocation siteLocation => new LocationResponse(
                siteLocation.Id,
                LocationType.Site,
                siteLocation.Site.ToResponse(),
                null,
                null),
            Location.BuildingLocation buildingLocation => new LocationResponse(
                buildingLocation.Id,
                LocationType.Building,
                buildingLocation.Site.ToResponse(),
                buildingLocation.Building.ToResponse(),
                null),
            Location.RoomLocation roomLocation => new LocationResponse(
                roomLocation.Id,
                LocationType.Room,
                roomLocation.Site.ToResponse(),
                roomLocation.Building.ToResponse(),
                roomLocation.Room.ToResponse()),
            _ => throw new InvalidOperationException("Unknown location type.")
        };
}
