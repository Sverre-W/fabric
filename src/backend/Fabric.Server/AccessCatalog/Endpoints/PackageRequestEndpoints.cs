using Fabric.Server.AccessCatalog.Application;
using Fabric.Server.AccessCatalog.Contracts;
using Fabric.Server.AccessCatalog.Domain;
using Fabric.Server.AccessCatalog.Persistence;
using Fabric.Server.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessCatalog.Endpoints;

public static class PackageRequestEndpoints
{
    public static IEndpointRouteBuilder MapPackageRequestEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder requests = app.MapGroup("/api/access-catalog/package-requests");

        requests.MapGet("", ListPackageRequests).Produces<Page<PackageRequestResponse>>();
        requests.MapPost("", CreatePackageRequest).Produces<PackageRequestResponse>(StatusCodes.Status201Created);
        requests.MapGet("/{requestId:guid}", GetPackageRequest).Produces<PackageRequestResponse>().Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListPackageRequests([AsParameters] ListPackageRequestsRequest request, [FromQuery] Guid[]? ids, AccessCatalogDbContext db, CancellationToken cancellationToken = default)
    {
        IQueryable<PackageRequest> query = db.PackageRequests.AsNoTracking();
        if (ids is { Length: > 0 })
            query = query.Where(item => ids.Contains(item.Id));
        if (request.RequesterIdentityId.HasValue)
            query = query.Where(item => item.RequesterIdentityId == request.RequesterIdentityId.Value);
        if (request.BeneficiaryIdentityId.HasValue)
            query = query.Where(item => item.BeneficiaryIdentityId == request.BeneficiaryIdentityId.Value);
        if (request.Status.HasValue)
            query = query.Where(item => item.Status == request.Status.Value);

        IPaged<PackageRequest> result = await query.OrderByDescending(item => item.CreatedAt).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        PackageRequest[] items = result.Items.ToArray();
        Dictionary<Guid, Guid[]> locations = await LoadLocationsAsync(db, items.Select(item => item.Id).ToArray(), cancellationToken);
        return Results.Ok(result.Map(item => item.ToResponse(locations.GetValueOrDefault(item.Id, []))));
    }

    private static async Task<IResult> CreatePackageRequest([FromBody] CreatePackageRequestRequest request, PackageRequestService service, AccessCatalogDbContext db, CancellationToken cancellationToken = default)
    {
        Result<PackageRequest, AccessCatalogErrors> result = await service.CreateAsync(request.PackageId, request.RequesterIdentityId, request.BeneficiaryIdentityId, request.LocationIds, request.RequestReason, request.DurationKind, request.ValidFrom, request.ValidUntil, cancellationToken);
        return await result.Match<Task<IResult>>(
            async item =>
            {
                Guid[] locationIds = await db.PackageRequestLocations.AsNoTracking().Where(link => link.RequestId == item.Id).Select(link => link.LocationId).ToArrayAsync(cancellationToken);
                return Results.Created($"/api/access-catalog/package-requests/{item.Id}", item.ToResponse(locationIds));
            },
            error => Task.FromResult(MapError(error).ToResult()));
    }

    private static async Task<IResult> GetPackageRequest(Guid requestId, AccessCatalogDbContext db, CancellationToken cancellationToken = default)
    {
        PackageRequest? request = await db.PackageRequests.AsNoTracking().SingleOrDefaultAsync(item => item.Id == requestId, cancellationToken);
        if (request is null)
            return Results.NotFound();

        Guid[] locationIds = await db.PackageRequestLocations.AsNoTracking().Where(link => link.RequestId == requestId).Select(link => link.LocationId).ToArrayAsync(cancellationToken);
        return Results.Ok(request.ToResponse(locationIds));
    }

    private static async Task<Dictionary<Guid, Guid[]>> LoadLocationsAsync(AccessCatalogDbContext db, Guid[] requestIds, CancellationToken cancellationToken)
    {
        return await db.PackageRequestLocations.AsNoTracking()
            .Where(item => requestIds.Contains(item.RequestId))
            .GroupBy(item => item.RequestId)
            .ToDictionaryAsync(group => group.Key, group => group.Select(item => item.LocationId).ToArray(), cancellationToken);
    }

    private static (int statusCode, ProblemDetails? problemDetails) MapError(AccessCatalogErrors error) => error switch
    {
        AccessCatalogErrors.PackageNotFound => Problem(StatusCodes.Status404NotFound, "Package not found."),
        AccessCatalogErrors.PackageInactive => Problem(StatusCodes.Status409Conflict, "Package is inactive."),
        AccessCatalogErrors.IdentityNotFound => Problem(StatusCodes.Status404NotFound, "Identity not found."),
        AccessCatalogErrors.PackageMustContainAccessItems => Problem(StatusCodes.Status409Conflict, "Package must contain at least one access item."),
        AccessCatalogErrors.LocationRequired => Problem(StatusCodes.Status400BadRequest, "At least one valid location is required."),
        AccessCatalogErrors.ReasonRequired => Problem(StatusCodes.Status400BadRequest, "Request reason is required."),
        AccessCatalogErrors.InvalidValidityRange => Problem(StatusCodes.Status400BadRequest, "Invalid approval window."),
        _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected access catalog error.")
    };

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) => (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
    private static IResult ToResult(this (int statusCode, ProblemDetails? problemDetails) error) => error.problemDetails is null ? Results.StatusCode(error.statusCode) : Results.Json(error.problemDetails, statusCode: error.statusCode);
}
