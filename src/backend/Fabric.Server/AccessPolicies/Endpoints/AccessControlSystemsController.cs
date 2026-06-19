using Fabric.Server.AccessPolicies.Application;
using Fabric.Server.AccessPolicies.Contracts;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Endpoints;

[ApiController]
public class AccessControlSystemsController
{
    [HttpGet("/api/access-policies/access-control-systems")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<AccessControlSystemResponse>))]
    [EndpointDescription("List access control systems")]
    [EndpointSummary("List access control systems")]
    public async Task<IResult> ListAccessControlSystems(
        [FromQuery] ListAccessControlSystemsRequest request,
        [FromQuery] Guid[]? ids,
        [FromServices] AccessPoliciesDbContext db,
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

    [HttpGet("/api/access-policies/access-control-systems/{systemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessControlSystemResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("Get an access control system")]
    [EndpointSummary("Get access control system")]
    public async Task<IResult> GetAccessControlSystem(
        Guid systemId,
        [FromServices] AccessPoliciesDbContext db,
        CancellationToken cancellationToken = default)
    {
        AccessControlSystem? system = await AccessControlSystemsWithDetails(db)
            .AsNoTracking()
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        return system is null ? Results.NotFound() : Results.Ok(system.ToResponse());
    }

    [HttpGet("/api/access-policies/access-control-systems/{systemId:guid}/metadata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SystemMetadata))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Fetch provider metadata for an access control system")]
    [EndpointSummary("Fetch access control system metadata")]
    public async Task<IResult> FetchMetadata(
        Guid systemId,
        [FromServices] AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<SystemMetadata, AccessControlSystemErrors> result = await service.FetchMetadata(systemId, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpGet("/api/access-policies/access-control-systems/{systemId:guid}/identity-mappings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<IdentityMappingResponse>))]
    [EndpointDescription("List identity mappings for an access control system")]
    [EndpointSummary("List identity mappings")]
    public async Task<IResult> ListIdentityMappings(
        Guid systemId,
        [FromQuery] ListIdentityMappingsRequest request,
        [FromQuery] Guid[]? subjectIds,
        [FromServices] AccessPoliciesDbContext db,
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

    [HttpPut("/api/access-policies/access-control-systems/{systemId:guid}/unipass/config")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Update Unipass system config")]
    [EndpointSummary("Update Unipass config")]
    public async Task<IResult> UpdateUnipassConfig(
        Guid systemId,
        [FromBody] UpdateUnipassConfigRequest request,
        [FromServices] AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<UnipassSystemConfig, AccessControlSystemErrors> config = UnipassSystemConfig.Create(
            request.Endpoint,
            request.SslValidation,
            request.Username,
            request.Password);

        if (config.IsFailure(out AccessControlSystemErrors error))
            return MapError(error).ToResult();

        config.IsSuccess(out UnipassSystemConfig value);
        Result<AccessControlSystemErrors> result = await service.UpdateUnipassConfig(systemId, value, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpPut("/api/access-policies/access-control-systems/{systemId:guid}/lenel/config")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Update Lenel system config")]
    [EndpointSummary("Update Lenel config")]
    public async Task<IResult> UpdateLenelConfig(
        Guid systemId,
        [FromBody] UpdateLenelConfigRequest request,
        [FromServices] AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<LenelSystemConfig, AccessControlSystemErrors> config = LenelSystemConfig.Create(
            request.Endpoint,
            request.SslValidation,
            request.ApiKey);

        if (config.IsFailure(out AccessControlSystemErrors error))
            return MapError(error).ToResult();

        config.IsSuccess(out LenelSystemConfig value);
        Result<AccessControlSystemErrors> result = await service.UpdateLenelConfig(systemId, value, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpPost("/api/access-policies/access-control-systems/{systemId:guid}/unipass/badge-types")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnipassBadgeTypeResponse))]
    public async Task<IResult> AddUnipassBadgeType(
        Guid systemId,
        [FromBody] AddUnipassBadgeTypeRequest request,
        [FromServices] AccessControlSystemService service,
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

    [HttpPost("/api/access-policies/access-control-systems/{systemId:guid}/lenel/badge-types")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LenelBadgeTypeResponse))]
    public async Task<IResult> AddLenelBadgeType(
        Guid systemId,
        [FromBody] AddLenelBadgeTypeRequest request,
        [FromServices] AccessControlSystemService service,
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

    [HttpDelete("/api/access-policies/access-control-systems/{systemId:guid}/badge-types/{badgeTypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RemoveBadgeType(
        Guid systemId,
        Guid badgeTypeId,
        [FromServices] AccessControlSystemService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessControlSystemErrors> result = await service.RemoveBadgeType(systemId, badgeTypeId, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpPost("/api/access-policies/access-control-systems/{systemId:guid}/unipass/access-level-types")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnipassAccessLevelTypeResponse))]
    public async Task<IResult> AddUnipassAccessLevel(
        Guid systemId,
        [FromBody] AddUnipassAccessLevelTypeRequest request,
        [FromServices] AccessControlSystemService service,
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

    [HttpPost("/api/access-policies/access-control-systems/{systemId:guid}/lenel/access-level-types")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LenelAccessLevelTypeResponse))]
    public async Task<IResult> AddLenelAccessLevel(
        Guid systemId,
        [FromBody] AddLenelAccessLevelTypeRequest request,
        [FromServices] AccessControlSystemService service,
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

    [HttpDelete("/api/access-policies/access-control-systems/{systemId:guid}/access-level-types/{accessLevelTypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> RemoveAccessLevel(
        Guid systemId,
        Guid accessLevelTypeId,
        [FromServices] AccessControlSystemService service,
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
