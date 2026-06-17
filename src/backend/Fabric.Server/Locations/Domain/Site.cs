using Fabric.Server.Core;

namespace Fabric.Server.Locations.Domain;

public sealed class Site
{
    private Site() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Address { get; private set; }
    public List<Building> Buildings { get; private set; } = [];

    public static Site Create(Guid id, string name, string? address) =>
        new()
        {
            Id = id,
            Name = name,
            Address = address,
            Buildings = []
        };

    public Location Rename(string name)
    {
        Name = name;
        return new Location.SiteLocation(this);
    }

    public Result<Location, LocationErrors> RenameBuilding(Guid buildingId, string name)
    {
        Building? building = FindBuildingById(buildingId);
        if (building is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingNotFound);

        if (Buildings.Any(x => x.Id != buildingId && x.Name == name))
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingAlreadyExists);

        building.Rename(name);
        return Result.Success<Location, LocationErrors>(new Location.BuildingLocation(this, building));
    }

    public Result<Location, LocationErrors> AddBuilding(string buildingName, string? buildingAddress)
    {
        if (Buildings.Any(building => building.Name == buildingName))
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingAlreadyExists);

        var building = Building.Create(Guid.NewGuid(), buildingName, buildingAddress);
        Buildings.Add(building);

        return Result.Success<Location, LocationErrors>(new Location.BuildingLocation(this, building));
    }

    public Result<Location, LocationErrors> RemoveBuilding(Guid buildingId)
    {
        Building? building = FindBuildingById(buildingId);
        if (building is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingNotFound);

        Buildings.Remove(building);

        return Result.Success<Location, LocationErrors>(new Location.SiteLocation(this));
    }

    public Result<Location, LocationErrors> AddRoom(
        Guid buildingId,
        string name,
        int capacity,
        bool wheelchairAccessible)
    {
        Building? building = FindBuildingById(buildingId);
        if (building is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingNotFound);

        if (building.Rooms.Any(room => room.Name == name))
            return Result.Failure<Location, LocationErrors>(LocationErrors.RoomAlreadyExists);

        var room = Room.Create(Guid.NewGuid(), name, capacity, wheelchairAccessible);
        building.AddRoom(room);

        return Result.Success<Location, LocationErrors>(new Location.RoomLocation(this, building, room));
    }

    public Result<Location, LocationErrors> UpdateRoom(
        Guid buildingId,
        Guid roomId,
        string name,
        int capacity,
        bool wheelchairAccessible)
    {
        Building? building = FindBuildingById(buildingId);
        if (building is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingNotFound);

        Room? room = FindRoomById(building, roomId);
        if (room is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.RoomNotFound);

        room.Update(name, capacity, wheelchairAccessible);

        return Result.Success<Location, LocationErrors>(new Location.RoomLocation(this, building, room));
    }

    public Result<Location, LocationErrors> RemoveRoom(Guid buildingId, Guid roomId)
    {
        Building? building = FindBuildingById(buildingId);
        if (building is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.BuildingNotFound);

        Room? room = FindRoomById(building, roomId);
        if (room is null)
            return Result.Failure<Location, LocationErrors>(LocationErrors.RoomNotFound);

        building.RemoveRoom(room);

        return Result.Success<Location, LocationErrors>(new Location.BuildingLocation(this, building));
    }

    private Building? FindBuildingById(Guid buildingId) =>
        Buildings.SingleOrDefault(building => building.Id == buildingId);

    private static Room? FindRoomById(Building building, Guid roomId) =>
        building.Rooms.SingleOrDefault(room => room.Id == roomId);
}
