using Fabric.Server.AccessPolicies.Application;
using Fabric.Server.AccessPolicies.Contracts;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Endpoints;

public static class AccessControlSystemEndpoints
{
    public static IEndpointRouteBuilder MapAccessControlSystemEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder systems = app.MapGroup("/api/access-policies/access-control-systems");

        systems.MapGet("", ListAccessControlSystems)
            .WithDescription("List access control systems")
            .WithSummary("List access control systems")
            .Produces<Page<AccessControlSystemResponse>>();
        systems.MapPost("", CreateAccessControlSystem)
            .WithDescription("Register access control system")
            .WithSummary("Register access control system")
            .Produces<AccessControlSystemResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        systems.MapGet("/{systemId:guid}", GetAccessControlSystem)
            .WithDescription("Get an access control system")
            .WithSummary("Get access control system")
            .Produces<AccessControlSystemResponse>()
            .Produces(StatusCodes.Status404NotFound);
        systems.MapGet("/{systemId:guid}/metadata", FetchMetadata)
            .WithDescription("Fetch provider metadata for an access control system")
            .WithSummary("Fetch access control system metadata")
            .Produces<SystemMetadata>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        systems.MapGet("/{systemId:guid}/identity-mappings", ListIdentityMappings)
            .WithDescription("List identity mappings for an access control system")
            .WithSummary("List identity mappings")
            .Produces<Page<IdentityMappingResponse>>();
        systems.MapPut("/{systemId:guid}/unipass/config", UpdateUnipassConfig)
            .WithDescription("Update Unipass system config")
            .WithSummary("Update Unipass config")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        systems.MapPut("/{systemId:guid}/lenel/config", UpdateLenelConfig)
            .WithDescription("Update Lenel system config")
            .WithSummary("Update Lenel config")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        systems.MapPost("/{systemId:guid}/unipass/badge-types", AddUnipassBadgeType)
            .Produces<UnipassBadgeTypeResponse>();
        systems.MapPost("/{systemId:guid}/lenel/badge-types", AddLenelBadgeType)
            .Produces<LenelBadgeTypeResponse>();
        systems.MapDelete("/{systemId:guid}/badge-types/{badgeTypeId:guid}", RemoveBadgeType)
            .Produces(StatusCodes.Status204NoContent);
        systems.MapPost("/{systemId:guid}/unipass/access-level-types", AddUnipassAccessLevel)
            .Produces<UnipassAccessLevelTypeResponse>();
        systems.MapPost("/{systemId:guid}/lenel/access-level-types", AddLenelAccessLevel)
            .Produces<LenelAccessLevelTypeResponse>();
        systems.MapDelete("/{systemId:guid}/access-level-types/{accessLevelTypeId:guid}", RemoveAccessLevel)
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }

    private static async Task<IResult> ListAccessControlSystems(
        [AsParameters] ListAccessControlSystemsRequest request,
        [FromQuery] Guid[]? ids,
        AccessPoliciesDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AccessControlSystem> query = AccessControlSystemsWithDetails(db).AsNoTracking();

        if (ids is { Length: > 0 })
            query = query.Where(system => ids.Contains(system.Id));

        if (!string.IsNullOrWhiteSpace(request.Name))
            query = query.Where(system => system.Name.ToLower().Contains(request.Name.ToLower()));

        IPaged<AccessControlSystem> result = await query
            .OrderBy(system => system.Name)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(result.Map(system => system.ToResponse()));
    }

    private static async Task<IResult> CreateAccessControlSystem(
        [FromBody] CreateAccessControlSystemRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessControlSystem, AccessControlSystemErrors> result;

        switch (request)
        {
            case CreateUnipassAccessControlSystemRequest unipass:
            {
                Result<UnipassSystemConfig, AccessControlSystemErrors> config = UnipassSystemConfig.Create(
                    unipass.Endpoint,
                    unipass.SslValidation,
                    unipass.Username,
                    unipass.Password);

                if (config.IsFailure(out AccessControlSystemErrors error))
                    return MapError(error).ToResult();

                config.IsSuccess(out UnipassSystemConfig value);
                result = await service.CreateUnipassSystem(unipass.Name, value, cancellationToken);
                break;
            }
            case CreateLenelAccessControlSystemRequest lenel:
            {
                Result<LenelSystemConfig, AccessControlSystemErrors> config = LenelSystemConfig.Create(
                    lenel.Endpoint,
                    lenel.SslValidation,
                    lenel.ApiKey);

                if (config.IsFailure(out AccessControlSystemErrors error))
                    return MapError(error).ToResult();

                config.IsSuccess(out LenelSystemConfig value);
                result = await service.CreateLenelSystem(lenel.Name, value, cancellationToken);
                break;
            }
            default:
                result = Result.Failure<AccessControlSystem, AccessControlSystemErrors>(AccessControlSystemErrors.SystemProviderMismatch);
                break;
        }

        return result.Match(
            system => Results.Created($"/api/access-policies/access-control-systems/{system.Id}", system.ToResponse()),
            error => MapError(error).ToResult());
    }

    private static async Task<IResult> GetAccessControlSystem(
        Guid systemId,
        AccessPoliciesDbContext db,
        CancellationToken cancellationToken = default)
    {
        AccessControlSystem? system = await AccessControlSystemsWithDetails(db)
            .AsNoTracking()
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        return system is null ? Results.NotFound() : Results.Ok(system.ToResponse());
    }

    private static async Task<IResult> FetchMetadata(
        Guid systemId,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<SystemMetadata, AccessControlSystemErrors> result = await service.FetchMetadata(systemId, cancellationToken);
        return result.AsResponse(MapError);
    }

    private static async Task<IResult> ListIdentityMappings(
        Guid systemId,
        [AsParameters] ListIdentityMappingsRequest request,
        [FromQuery] Guid[]? subjectIds,
        AccessPoliciesDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<IdentityMapping> query = db.IdentityMappings
            .AsNoTracking()
            .Where(mapping => mapping.SystemId == systemId);

        if (subjectIds is { Length: > 0 })
            query = query.Where(mapping => subjectIds.Contains(mapping.SubjectId));

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            string name = request.Name.ToLower();
            query = query.Where(mapping =>
                mapping.FirstName.ToLower().Contains(name) ||
                mapping.LastName.ToLower().Contains(name));
        }

        IPaged<IdentityMapping> result = await query
            .OrderBy(mapping => mapping.LastName)
            .ThenBy(mapping => mapping.FirstName)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(result.Map(mapping => mapping.ToResponse()));
    }

    private static async Task<IResult> UpdateUnipassConfig(
        Guid systemId,
        [FromBody] UpdateUnipassConfigRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessControlSystemErrors> result = await service.UpdateUnipassConfig(
            systemId,
            request.Endpoint,
            request.SslValidation,
            request.Username,
            request.Password,
            cancellationToken);
        return result.AsResponse(MapError);
    }

    private static async Task<IResult> UpdateLenelConfig(
        Guid systemId,
        [FromBody] UpdateLenelConfigRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessControlSystemErrors> result = await service.UpdateLenelConfig(
            systemId,
            request.Endpoint,
            request.SslValidation,
            request.ApiKey,
            cancellationToken);
        return result.AsResponse(MapError);
    }

    private static async Task<IResult> AddUnipassBadgeType(
        Guid systemId,
        [FromBody] AddUnipassBadgeTypeRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<UnipassBadgeType, AccessControlSystemErrors> result = await service.AddUnipassBadgeType(
            systemId,
            request.Name,
            request.RangeStart,
            request.RangeStop,
            cancellationToken);

        return result.Map(type => type.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> AddLenelBadgeType(
        Guid systemId,
        [FromBody] AddLenelBadgeTypeRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<LenelBadgeType, AccessControlSystemErrors> result = await service.AddLenelBadgeType(
            systemId,
            request.Name,
            request.BadgeTypeId,
            request.Metadata,
            cancellationToken);

        return result.Map(type => type.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> RemoveBadgeType(
        Guid systemId,
        Guid badgeTypeId,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessControlSystemErrors> result = await service.RemoveBadgeType(systemId, badgeTypeId, cancellationToken);
        return result.AsResponse(MapError);
    }

    private static async Task<IResult> AddUnipassAccessLevel(
        Guid systemId,
        [FromBody] AddUnipassAccessLevelTypeRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<UnipassAccessLevelType, AccessControlSystemErrors> result = await service.AddUnipassAccessLevel(
            systemId,
            request.Name,
            request.SiteId,
            request.AccessRuleId,
            request.Metadata,
            cancellationToken);

        return result.Map(type => type.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> AddLenelAccessLevel(
        Guid systemId,
        [FromBody] AddLenelAccessLevelTypeRequest request,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<LenelAccessLevelType, AccessControlSystemErrors> result = await service.AddLenelAccessLevel(
            systemId,
            request.Name,
            request.AccessLevelId,
            request.BadgeTypeIds,
            request.Metadata,
            cancellationToken);

        return result.Map(type => type.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> RemoveAccessLevel(
        Guid systemId,
        Guid accessLevelTypeId,
        AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessControlSystemErrors> result = await service.RemoveAccessLevel(systemId, accessLevelTypeId, cancellationToken);
        return result.AsResponse(MapError);
    }

    private static IQueryable<AccessControlSystem> AccessControlSystemsWithDetails(AccessPoliciesDbContext db) =>
        db.AccessControlSystems
            .Include(system => ((UnipassAccessControlSystem)system).BadgeTypes)
            .Include(system => ((UnipassAccessControlSystem)system).AccessLevels)
            .Include(system => ((LenelAccessControlSystem)system).BadgeTypes)
            .Include(system => ((LenelAccessControlSystem)system).AccessLevels)
                .ThenInclude(accessLevel => accessLevel.BadgeTypes);

    private static (int statusCode, ProblemDetails? problemDetails) MapError(AccessControlSystemErrors error) =>
        error switch
        {
            AccessControlSystemErrors.SystemNotFound => Problem(StatusCodes.Status404NotFound, "System not found."),
            AccessControlSystemErrors.BadgeTypeNotFound => Problem(StatusCodes.Status404NotFound, "Badge type not found."),
            AccessControlSystemErrors.AccessLevelTypeNotFound => Problem(StatusCodes.Status404NotFound, "Access level type not found."),
            AccessControlSystemErrors.SystemProviderMismatch => Problem(StatusCodes.Status400BadRequest, "System provider mismatch."),
            AccessControlSystemErrors.BadgeRangeInvalid => Problem(StatusCodes.Status400BadRequest, "Badge range invalid."),
            AccessControlSystemErrors.ConfigInvalid => Problem(StatusCodes.Status400BadRequest, "Config invalid."),
            AccessControlSystemErrors.LenelBadgeTypesNotFound => Problem(StatusCodes.Status400BadRequest, "Lenel badge types not found."),
            AccessControlSystemErrors.SiteNotFoundInMetadata => Problem(StatusCodes.Status400BadRequest, "Site not found in metadata."),
            AccessControlSystemErrors.AccessRuleNotFoundInMetadata => Problem(StatusCodes.Status400BadRequest, "Access rule not found in metadata."),
            AccessControlSystemErrors.BadgeTypeNotFoundInMetadata => Problem(StatusCodes.Status400BadRequest, "Badge type not found in metadata."),
            AccessControlSystemErrors.AccessLevelNotFoundInMetadata => Problem(StatusCodes.Status400BadRequest, "Access level not found in metadata."),
            AccessControlSystemErrors.BadgeTypeAlreadyExists => Problem(StatusCodes.Status409Conflict, "Badge type already exists."),
            AccessControlSystemErrors.AccessLevelTypeAlreadyExists => Problem(StatusCodes.Status409Conflict, "Access level type already exists."),
            AccessControlSystemErrors.BadgeTypeInUse => Problem(StatusCodes.Status409Conflict, "Badge type is in use."),
            AccessControlSystemErrors.AccessLevelTypeInUse => Problem(StatusCodes.Status409Conflict, "Access level type is in use."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected access control system error.")
        };

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) =>
        (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
