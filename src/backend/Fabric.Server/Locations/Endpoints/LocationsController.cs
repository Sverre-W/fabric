using Fabric.Server.Core;
using Fabric.Server.Locations.Application;
using Fabric.Server.Locations.Contracts;
using Fabric.Server.Locations.Domain;
using Fabric.Server.Locations.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Locations.Endpoints;

[ApiController]
public class LocationsController
{
    [HttpGet("/api/locations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("Retrieve a site, building, or room by id")]
    [EndpointSummary("Retrieve a location by id")]
    public async Task<IResult> GetLocationById(
        Guid id,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Location? location = await locationService.GetLocationById(id, cancellationToken);
        return location is null ? Results.NotFound() : Results.Ok(location.ToResponse());
    }

    [HttpGet("/api/sites")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<SiteResponse>))]
    [EndpointDescription("List all sites")]
    [EndpointSummary("List sites")]
    public async Task<IResult> ListSites(
        [FromQuery] ListSitesRequest request,
        [FromServices] LocationsDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Site> query = db.Sites.AsNoTracking().OrderBy(site => site.Name);
        IPaged<Site> result = await query.GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(site => site.ToResponse()));
    }

    [HttpGet("/api/sites/{siteId:guid}/buildings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<BuildingResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("List buildings for a site")]
    [EndpointSummary("List site buildings")]
    public async Task<IResult> ListBuildings(
        Guid siteId,
        [FromServices] LocationsDbContext db,
        CancellationToken cancellationToken = default)
    {
        Site? site = await db.Sites
            .AsNoTracking()
            .Include(x => x.Buildings)
            .SingleOrDefaultAsync(x => x.Id == siteId, cancellationToken);

        if (site is null)
            return Results.NotFound();

        return Results.Ok(site.Buildings.OrderBy(x => x.Name).Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("/api/sites/{siteId:guid}/buildings/{buildingId:guid}/rooms")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyCollection<RoomResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("List rooms for a building")]
    [EndpointSummary("List building rooms")]
    public async Task<IResult> ListRooms(
        Guid siteId,
        Guid buildingId,
        [FromServices] LocationsDbContext db,
        CancellationToken cancellationToken = default)
    {
        Site? site = await db.Sites
            .AsNoTracking()
            .Include(x => x.Buildings)
            .ThenInclude(x => x.Rooms)
            .SingleOrDefaultAsync(x => x.Id == siteId, cancellationToken);

        Building? building = site?.Buildings.SingleOrDefault(x => x.Id == buildingId);
        if (building is null)
            return Results.NotFound();

        return Results.Ok(building.Rooms.OrderBy(x => x.Name).Select(x => x.ToResponse()).ToArray());
    }

    [HttpPost("/api/sites")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LocationResponse))]
    [EndpointDescription("Create a site")]
    [EndpointSummary("Create site")]
    public async Task<IResult> CreateSite(
        [FromBody] CreateSiteRequest request,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.CreateSite(
            request.Id ?? Guid.NewGuid(),
            request.Name,
            request.Address,
            cancellationToken);

        return result.Match(
            location => Results.Created($"/api/locations/{location.Id}", location.ToResponse()),
            error => MapError(error).ToResult());
    }

    [HttpPost("/api/sites/{siteId:guid}/buildings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Add a building to a site")]
    [EndpointSummary("Add building")]
    public async Task<IResult> AddBuilding(
        Guid siteId,
        [FromBody] AddBuildingRequest request,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.AddBuilding(
            siteId,
            request.Name,
            request.Address,
            cancellationToken);

        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    [HttpPut("/api/sites/{siteId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Update a site's name")]
    [EndpointSummary("Update site")]
    public async Task<IResult> UpdateSite(
        Guid siteId,
        [FromBody] UpdateSiteRequest request,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.UpdateSite(siteId, request.Name, cancellationToken);
        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    [HttpPut("/api/sites/{siteId:guid}/buildings/{buildingId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Update a building's name")]
    [EndpointSummary("Update building")]
    public async Task<IResult> UpdateBuilding(
        Guid siteId,
        Guid buildingId,
        [FromBody] UpdateBuildingRequest request,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.UpdateBuilding(
            siteId,
            buildingId,
            request.Name,
            cancellationToken);

        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    [HttpDelete("/api/sites/{siteId:guid}/buildings/{buildingId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Remove a building from a site")]
    [EndpointSummary("Remove building")]
    public async Task<IResult> RemoveBuilding(
        Guid siteId,
        Guid buildingId,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.RemoveBuilding(siteId, buildingId, cancellationToken);
        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    [HttpPost("/api/sites/{siteId:guid}/buildings/{buildingId:guid}/rooms")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Add a room to a building")]
    [EndpointSummary("Add room")]
    public async Task<IResult> AddRoom(
        Guid siteId,
        Guid buildingId,
        [FromBody] AddRoomRequest request,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.AddRoom(
            siteId,
            buildingId,
            request.Name,
            request.Capacity,
            request.WheelchairAccessible,
            cancellationToken);

        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    [HttpPut("/api/sites/{siteId:guid}/buildings/{buildingId:guid}/rooms/{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Update a room")]
    [EndpointSummary("Update room")]
    public async Task<IResult> UpdateRoom(
        Guid siteId,
        Guid buildingId,
        Guid roomId,
        [FromBody] UpdateRoomRequest request,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.UpdateRoom(
            siteId,
            buildingId,
            roomId,
            request.Name,
            request.Capacity,
            request.WheelchairAccessible,
            cancellationToken);

        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    [HttpDelete("/api/sites/{siteId:guid}/buildings/{buildingId:guid}/rooms/{roomId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LocationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Remove a room from a building")]
    [EndpointSummary("Remove room")]
    public async Task<IResult> RemoveRoom(
        Guid siteId,
        Guid buildingId,
        Guid roomId,
        [FromServices] LocationService locationService,
        CancellationToken cancellationToken = default)
    {
        Result<Location, LocationErrors> result = await locationService.RemoveRoom(siteId, buildingId, roomId, cancellationToken);
        return result.Map(location => location.ToResponse()).AsResponse(MapError);
    }

    private static (int statusCode, ProblemDetails? problemDetails) MapError(LocationErrors error)
    {
        return error switch
        {
            LocationErrors.SiteNotFound => Problem(StatusCodes.Status404NotFound, "Site not found."),
            LocationErrors.BuildingNotFound => Problem(StatusCodes.Status404NotFound, "Building not found."),
            LocationErrors.RoomNotFound => Problem(StatusCodes.Status404NotFound, "Room not found."),
            LocationErrors.BuildingAlreadyExists => Problem(StatusCodes.Status409Conflict, "Building already exists."),
            LocationErrors.RoomAlreadyExists => Problem(StatusCodes.Status409Conflict, "Room already exists."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected location error.")
        };
    }

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) =>
        (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}

file static class LocationControllerResultExtensions
{
    public static IResult ToResult(this (int statusCode, ProblemDetails? problemDetails) error) =>
        error.problemDetails is null
            ? Results.StatusCode(error.statusCode)
            : Results.Json(error.problemDetails, statusCode: error.statusCode);
}
