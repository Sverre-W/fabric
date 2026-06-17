using Fabric.Server.Core;

namespace Fabric.Server.Locations.Contracts;

public record ListSitesRequest : BaseListRequest;

public record CreateSiteRequest(
    Guid? Id,
    string Name,
    string? Address
);

public record UpdateSiteRequest(
    string Name
);

public record AddBuildingRequest(
    string Name,
    string? Address
);

public record UpdateBuildingRequest(
    string Name
);

public record AddRoomRequest(
    string Name,
    int Capacity,
    bool WheelchairAccessible
);

public record UpdateRoomRequest(
    string Name,
    int Capacity,
    bool WheelchairAccessible
);
