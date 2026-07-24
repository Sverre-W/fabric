using Fabric.Server.Core;
using Fabric.Server.CredentialManagement.Application;
using Fabric.Server.CredentialManagement.Contracts;
using Fabric.Server.CredentialManagement.Domain;
using Fabric.Server.CredentialManagement.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.CredentialManagement.Endpoints;

public static class CredentialManagementEndpoints
{
    public static IEndpointRouteBuilder MapCredentialManagementEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder credentialTypes = app.MapGroup("/api/credential-management/credential-types");

        credentialTypes.MapGet("", ListCredentialTypes)
            .WithSummary("List credential types")
            .Produces<Page<CredentialTypeResponse>>();
        credentialTypes.MapGet("/{id:guid}", GetCredentialType)
            .WithSummary("Get credential type")
            .Produces<CredentialTypeResponse>()
            .Produces(StatusCodes.Status404NotFound);
        credentialTypes.MapPost("", CreateCredentialType)
            .WithSummary("Create credential type")
            .Produces<CredentialTypeResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        credentialTypes.MapPut("/{id:guid}", UpdateCredentialType)
            .WithSummary("Update credential type")
            .Produces<CredentialTypeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        credentialTypes.MapPost("/{id:guid}/activate", ActivateCredentialType)
            .WithSummary("Activate credential type")
            .Produces<CredentialTypeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        credentialTypes.MapPost("/{id:guid}/disable", DisableCredentialType)
            .WithSummary("Disable credential type")
            .Produces<CredentialTypeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        credentialTypes.MapPost("/{id:guid}/ranges", CreateCredentialRange)
            .WithSummary("Create credential range")
            .Produces<CredentialRangeResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        credentialTypes.MapPut("/ranges/{rangeId:guid}", UpdateCredentialRange)
            .WithSummary("Update credential range")
            .Produces<CredentialRangeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        RouteGroupBuilder credentials = app.MapGroup("/api/credential-management/credentials");
        credentials.MapGet("", ListCredentials)
            .WithSummary("List credentials")
            .Produces<Page<CredentialResponse>>();
        credentials.MapGet("/{id:guid}", GetCredential)
            .WithSummary("Get credential")
            .Produces<CredentialResponse>()
            .Produces(StatusCodes.Status404NotFound);
        credentials.MapPost("", IssueCredential)
            .WithSummary("Issue credential")
            .Produces<CredentialResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> ListCredentialTypes(
        [AsParameters] ListCredentialTypesRequest request,
        [FromQuery] Guid[]? ids,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CredentialType> query = db.CredentialTypes.Include(type => type.Ranges).AsNoTracking();

        if (ids is { Length: > 0 })
            query = query.Where(type => ids.Contains(type.Id));

        if (request.Technology.HasValue)
            query = query.Where(type => type.Technology == request.Technology.Value);

        if (request.Status.HasValue)
            query = query.Where(type => type.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            string filter = $"%{request.Query}%";
            query = query.Where(type => EF.Functions.ILike(type.Name, filter));
        }

        IPaged<CredentialType> page = await query
            .OrderBy(type => type.Name)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        Dictionary<Guid, (int Used, int Available)> capacity = await GetCapacityAsync(page.Items.Select(type => type.Id).ToArray(), db, cancellationToken);
        return Results.Ok(page.Map(type => type.ToResponse(capacity.GetValueOrDefault(type.Id).Used, capacity.GetValueOrDefault(type.Id).Available)));
    }

    private static async Task<IResult> GetCredentialType(
        Guid id,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        CredentialType? credentialType = await db.CredentialTypes
            .Include(type => type.Ranges)
            .AsNoTracking()
            .SingleOrDefaultAsync(type => type.Id == id, cancellationToken);
        if (credentialType is null)
            return Results.NotFound();

        (int used, int available) = await GetCapacityAsync(id, db, cancellationToken);
        return Results.Ok(credentialType.ToResponse(used, available));
    }

    private static async Task<IResult> CreateCredentialType(
        [FromBody] CreateCredentialTypeRequest request,
        CredentialManagementService service,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialType, CredentialManagementErrors> result = await service.CreateCredentialTypeAsync(request, cancellationToken);
        if (result.IsFailure(out CredentialManagementErrors error))
            return ToResult(MapError(error));

        result.IsSuccess(out CredentialType credentialType);
        (int used, int available) = await GetCapacityAsync(credentialType.Id, db, cancellationToken);
        return Results.Created($"/api/credential-management/credential-types/{credentialType.Id}", credentialType.ToResponse(used, available));
    }

    private static async Task<IResult> UpdateCredentialType(
        Guid id,
        [FromBody] UpdateCredentialTypeRequest request,
        CredentialManagementService service,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialType, CredentialManagementErrors> result = await service.UpdateCredentialTypeAsync(id, request, cancellationToken);
        if (result.IsFailure(out CredentialManagementErrors error))
            return ToResult(MapError(error));

        result.IsSuccess(out CredentialType credentialType);
        (int used, int available) = await GetCapacityAsync(credentialType.Id, db, cancellationToken);
        return Results.Ok(credentialType.ToResponse(used, available));
    }

    private static Task<IResult> ActivateCredentialType(
        Guid id,
        CredentialManagementService service,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default) =>
        SetCredentialTypeStatus(id, CredentialTypeStatus.Active, service, db, cancellationToken);

    private static Task<IResult> DisableCredentialType(
        Guid id,
        CredentialManagementService service,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default) =>
        SetCredentialTypeStatus(id, CredentialTypeStatus.Disabled, service, db, cancellationToken);

    private static async Task<IResult> SetCredentialTypeStatus(
        Guid id,
        CredentialTypeStatus status,
        CredentialManagementService service,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken)
    {
        Result<CredentialType, CredentialManagementErrors> result = await service.SetCredentialTypeStatusAsync(id, status, cancellationToken);
        if (result.IsFailure(out CredentialManagementErrors error))
            return ToResult(MapError(error));

        result.IsSuccess(out CredentialType credentialType);
        (int used, int available) = await GetCapacityAsync(credentialType.Id, db, cancellationToken);
        return Results.Ok(credentialType.ToResponse(used, available));
    }

    private static async Task<IResult> CreateCredentialRange(
        Guid id,
        [FromBody] CreateCredentialRangeRequest request,
        CredentialManagementService service,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialRange, CredentialManagementErrors> result = await service.CreateCredentialRangeAsync(id, request, cancellationToken);
        return result.Match<IResult>(
            range => Results.Created($"/api/credential-management/credential-types/ranges/{range.Id}", range.ToResponse()),
            error => ToResult(MapError(error)));
    }

    private static async Task<IResult> UpdateCredentialRange(
        Guid rangeId,
        [FromBody] UpdateCredentialRangeRequest request,
        CredentialManagementService service,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialRange, CredentialManagementErrors> result = await service.UpdateCredentialRangeAsync(rangeId, request, cancellationToken);
        return result.Match<IResult>(range => Results.Ok(range.ToResponse()), error => ToResult(MapError(error)));
    }

    private static async Task<IResult> ListCredentials(
        [AsParameters] ListCredentialsRequest request,
        [FromQuery] Guid[]? ids,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Credential> query = db.Credentials.AsNoTracking();

        if (ids is { Length: > 0 })
            query = query.Where(credential => ids.Contains(credential.Id));
        if (request.CredentialTypeId.HasValue)
            query = query.Where(credential => credential.CredentialTypeId == request.CredentialTypeId.Value);
        if (request.IdentityId.HasValue)
            query = query.Where(credential => credential.IdentityId == request.IdentityId.Value);
        if (request.Status.HasValue)
            query = query.Where(credential => credential.Status == request.Status.Value);

        IPaged<Credential> page = await query
            .OrderByDescending(credential => credential.CreatedAt)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(page.Map(credential => credential.ToResponse()));
    }

    private static async Task<IResult> GetCredential(
        Guid id,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        Credential? credential = await db.Credentials.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return credential is null ? Results.NotFound() : Results.Ok(credential.ToResponse());
    }

    private static async Task<IResult> IssueCredential(
        [FromBody] IssueCredentialRequest request,
        CredentialManagementService service,
        CancellationToken cancellationToken = default)
    {
        Result<Credential, CredentialManagementErrors> result = await service.IssueCredentialAsync(request, cancellationToken);
        return result.Match<IResult>(
            credential => Results.Created($"/api/credential-management/credentials/{credential.Id}", credential.ToResponse()),
            error => ToResult(MapError(error)));
    }

    private static async Task<Dictionary<Guid, (int Used, int Available)>> GetCapacityAsync(
        Guid[] credentialTypeIds,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, int> used = await db.Credentials
            .Where(credential => credentialTypeIds.Contains(credential.CredentialTypeId))
            .GroupBy(credential => credential.CredentialTypeId)
            .Select(group => new { CredentialTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CredentialTypeId, item => item.Count, cancellationToken);

        Dictionary<Guid, int> available = [];
        CredentialRange[] ranges = await db.CredentialRanges
            .Where(range => credentialTypeIds.Contains(range.CredentialTypeId) && range.IsActive)
            .ToArrayAsync(cancellationToken);

        foreach (IGrouping<Guid, CredentialRange> group in ranges.GroupBy(range => range.CredentialTypeId))
        {
            long total = group.Sum(range => range.RangeStop - range.RangeStart + 1);
            available[group.Key] = (int)Math.Max(0, total - used.GetValueOrDefault(group.Key));
        }

        return credentialTypeIds.ToDictionary(id => id, id => (used.GetValueOrDefault(id), available.GetValueOrDefault(id)));
    }

    private static async Task<(int Used, int Available)> GetCapacityAsync(
        Guid credentialTypeId,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, (int Used, int Available)> counts = await GetCapacityAsync([credentialTypeId], db, cancellationToken);
        return counts.GetValueOrDefault(credentialTypeId);
    }

    private static IResult ToResult((int statusCode, ProblemDetails problemDetails) error) =>
        Results.Json(error.problemDetails, statusCode: error.statusCode);

    private static (int statusCode, ProblemDetails problemDetails) MapError(CredentialManagementErrors error) =>
        error switch
        {
            CredentialManagementErrors.CredentialTypeNotFound => Problem(StatusCodes.Status404NotFound, "Credential type not found."),
            CredentialManagementErrors.CredentialRangeNotFound => Problem(StatusCodes.Status404NotFound, "Credential range not found."),
            CredentialManagementErrors.CredentialNotFound => Problem(StatusCodes.Status404NotFound, "Credential not found."),
            CredentialManagementErrors.CredentialTypeAlreadyExists => Problem(StatusCodes.Status409Conflict, "Credential type already exists."),
            CredentialManagementErrors.CredentialIdentifierAlreadyExists => Problem(StatusCodes.Status409Conflict, "Credential identifier already exists."),
            CredentialManagementErrors.CredentialIdentifierUnavailable => Problem(StatusCodes.Status409Conflict, "Credential identifier is unavailable."),
            CredentialManagementErrors.CredentialTypeDisabled => Problem(StatusCodes.Status409Conflict, "Credential type is disabled."),
            CredentialManagementErrors.CredentialTypeInvalid => Problem(StatusCodes.Status400BadRequest, "Credential type is invalid."),
            CredentialManagementErrors.CredentialRangeInvalid => Problem(StatusCodes.Status400BadRequest, "Credential range is invalid."),
            CredentialManagementErrors.CredentialIdentifierOutsideRange => Problem(StatusCodes.Status400BadRequest, "Credential identifier is outside the active credential ranges."),
            CredentialManagementErrors.CredentialIdentifierMustBeNumeric => Problem(StatusCodes.Status400BadRequest, "Credential identifier must be numeric for range allocation."),
            CredentialManagementErrors.CredentialIdentifierRequired => Problem(StatusCodes.Status400BadRequest, "Credential identifier is required."),
            CredentialManagementErrors.TemporaryCredentialRequiresValidUntil => Problem(StatusCodes.Status400BadRequest, "Temporary credentials require a valid until value."),
            CredentialManagementErrors.PermanentCredentialMustNotHaveValidUntil => Problem(StatusCodes.Status400BadRequest, "Permanent credentials must not have a valid until value."),
            CredentialManagementErrors.ValidUntilMustBeAfterValidFrom => Problem(StatusCodes.Status400BadRequest, "Valid until must be after valid from."),
            CredentialManagementErrors.ReasonRequired => Problem(StatusCodes.Status400BadRequest, "Reason text is required."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected credential management error.")
        };

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) =>
        (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
