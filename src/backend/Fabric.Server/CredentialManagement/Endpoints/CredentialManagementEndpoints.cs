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
        credentialTypes.MapPost("/{id:guid}/targets", CreateCredentialTypeTarget)
            .WithSummary("Create credential type target")
            .Produces<CredentialTypeTargetResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        credentialTypes.MapPut("/targets/{targetId:guid}", UpdateCredentialTypeTarget)
            .WithSummary("Update credential type target")
            .Produces<CredentialTypeTargetResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        credentialTypes.MapPost("/targets/{targetId:guid}/enable", EnableCredentialTypeTarget)
            .WithSummary("Enable credential type target")
            .Produces<CredentialTypeTargetResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        credentialTypes.MapPost("/targets/{targetId:guid}/disable", DisableCredentialTypeTarget)
            .WithSummary("Disable credential type target")
            .Produces<CredentialTypeTargetResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        RouteGroupBuilder reservations = app.MapGroup("/api/credential-management/reservations");
        reservations.MapGet("", ListCredentialReservations)
            .WithSummary("List credential reservations")
            .Produces<Page<CredentialReservationResponse>>();
        reservations.MapPost("", ReserveCredential)
            .WithSummary("Reserve credential")
            .Produces<CredentialReservationResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

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
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CredentialType> query = db.CredentialTypes.Include(type => type.Targets).AsNoTracking();

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

        Dictionary<Guid, int> usedCounts = await GetUsedCountsAsync(page.Items.Select(type => type.Id).ToArray(), db, cancellationToken);
        return Results.Ok(page.Map(type => type.ToResponse(usedCounts.GetValueOrDefault(type.Id))));
    }

    private static async Task<IResult> GetCredentialType(
        Guid id,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        CredentialType? credentialType = await db.CredentialTypes
            .Include(type => type.Targets)
            .AsNoTracking()
            .SingleOrDefaultAsync(type => type.Id == id, cancellationToken);
        if (credentialType is null)
            return Results.NotFound();

        int usedCount = await GetUsedCountAsync(id, db, cancellationToken);
        return Results.Ok(credentialType.ToResponse(usedCount));
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
        int usedCount = await GetUsedCountAsync(credentialType.Id, db, cancellationToken);
        return Results.Created($"/api/credential-management/credential-types/{credentialType.Id}", credentialType.ToResponse(usedCount));
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
        int usedCount = await GetUsedCountAsync(credentialType.Id, db, cancellationToken);
        return Results.Ok(credentialType.ToResponse(usedCount));
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
        int usedCount = await GetUsedCountAsync(credentialType.Id, db, cancellationToken);
        return Results.Ok(credentialType.ToResponse(usedCount));
    }

    private static async Task<IResult> CreateCredentialTypeTarget(
        Guid id,
        [FromBody] CreateCredentialTypeTargetRequest request,
        CredentialManagementService service,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialTypeTarget, CredentialManagementErrors> result = await service.CreateCredentialTypeTargetAsync(id, request, cancellationToken);
        return result.Match<IResult>(
            target => Results.Created($"/api/credential-management/credential-types/targets/{target.Id}", target.ToResponse()),
            error => ToResult(MapError(error)));
    }

    private static async Task<IResult> UpdateCredentialTypeTarget(
        Guid targetId,
        [FromBody] UpdateCredentialTypeTargetRequest request,
        CredentialManagementService service,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialTypeTarget, CredentialManagementErrors> result = await service.UpdateCredentialTypeTargetAsync(targetId, request, cancellationToken);
        return result.Match<IResult>(target => Results.Ok(target.ToResponse()), error => ToResult(MapError(error)));
    }

    private static Task<IResult> EnableCredentialTypeTarget(
        Guid targetId,
        CredentialManagementService service,
        CancellationToken cancellationToken = default) =>
        SetCredentialTypeTargetEnabled(targetId, true, service, cancellationToken);

    private static Task<IResult> DisableCredentialTypeTarget(
        Guid targetId,
        CredentialManagementService service,
        CancellationToken cancellationToken = default) =>
        SetCredentialTypeTargetEnabled(targetId, false, service, cancellationToken);

    private static async Task<IResult> SetCredentialTypeTargetEnabled(
        Guid targetId,
        bool enabled,
        CredentialManagementService service,
        CancellationToken cancellationToken)
    {
        Result<CredentialTypeTarget, CredentialManagementErrors> result = await service.SetCredentialTypeTargetEnabledAsync(targetId, enabled, cancellationToken);
        return result.Match<IResult>(target => Results.Ok(target.ToResponse()), error => ToResult(MapError(error)));
    }

    private static async Task<IResult> ListCredentialReservations(
        [AsParameters] ListCredentialReservationsRequest request,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<CredentialReservation> query = db.CredentialReservations.AsNoTracking();

        if (request.CredentialTypeId.HasValue)
            query = query.Where(reservation => reservation.CredentialTypeId == request.CredentialTypeId.Value);
        if (request.IdentityId.HasValue)
            query = query.Where(reservation => reservation.IdentityId == request.IdentityId.Value);
        if (request.Status.HasValue)
            query = query.Where(reservation => reservation.Status == request.Status.Value);

        IPaged<CredentialReservation> page = await query
            .OrderByDescending(reservation => reservation.CreatedAt)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(page.Map(reservation => reservation.ToResponse()));
    }

    private static async Task<IResult> ReserveCredential(
        [FromBody] CreateCredentialReservationRequest request,
        CredentialManagementService service,
        CancellationToken cancellationToken = default)
    {
        Result<CredentialReservation, CredentialManagementErrors> result = await service.ReserveCredentialAsync(request, cancellationToken);
        return result.Match<IResult>(
            reservation => Results.Created($"/api/credential-management/reservations/{reservation.Id}", reservation.ToResponse()),
            error => ToResult(MapError(error)));
    }

    private static async Task<IResult> ListCredentials(
        [AsParameters] ListCredentialsRequest request,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Credential> query = db.Credentials.AsNoTracking();

        if (request.CredentialTypeId.HasValue)
            query = query.Where(credential => credential.CredentialTypeId == request.CredentialTypeId.Value);
        if (request.IdentityId.HasValue)
            query = query.Where(credential => credential.IdentityId == request.IdentityId.Value);
        if (request.Status.HasValue)
            query = query.Where(credential => credential.Status == request.Status.Value);

        IPaged<Credential> page = await query
            .OrderByDescending(credential => credential.CreatedAt)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        Dictionary<Guid, CredentialProvisioningTransactionResponse[]> provisioning = await GetProvisioningAsync(page.Items.Select(credential => credential.Id).ToArray(), db, cancellationToken);
        return Results.Ok(page.Map(credential => credential.ToResponse(provisioning.GetValueOrDefault(credential.Id, []))));
    }

    private static async Task<IResult> GetCredential(
        Guid id,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        Credential? credential = await db.Credentials.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (credential is null)
            return Results.NotFound();

        Dictionary<Guid, CredentialProvisioningTransactionResponse[]> provisioning = await GetProvisioningAsync([id], db, cancellationToken);
        return Results.Ok(credential.ToResponse(provisioning.GetValueOrDefault(id, [])));
    }

    private static async Task<IResult> IssueCredential(
        [FromBody] IssueCredentialRequest request,
        CredentialManagementService service,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken = default)
    {
        Result<Credential, CredentialManagementErrors> result = await service.IssueCredentialAsync(request, cancellationToken);
        if (result.IsFailure(out CredentialManagementErrors error))
            return ToResult(MapError(error));

        result.IsSuccess(out Credential credential);
        Dictionary<Guid, CredentialProvisioningTransactionResponse[]> provisioning = await GetProvisioningAsync([credential.Id], db, cancellationToken);
        return Results.Created(
            $"/api/credential-management/credentials/{credential.Id}",
            credential.ToResponse(provisioning.GetValueOrDefault(credential.Id, [])));
    }

    private static async Task<Dictionary<Guid, int>> GetUsedCountsAsync(
        Guid[] credentialTypeIds,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, int> issued = await db.Credentials
            .Where(credential => credentialTypeIds.Contains(credential.CredentialTypeId))
            .GroupBy(credential => credential.CredentialTypeId)
            .Select(group => new { CredentialTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CredentialTypeId, item => item.Count, cancellationToken);

        Dictionary<Guid, int> reserved = await db.CredentialReservations
            .Where(reservation => credentialTypeIds.Contains(reservation.CredentialTypeId) && reservation.Status == CredentialReservationStatus.Active)
            .GroupBy(reservation => reservation.CredentialTypeId)
            .Select(group => new { CredentialTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CredentialTypeId, item => item.Count, cancellationToken);

        foreach ((Guid credentialTypeId, int count) in reserved)
            issued[credentialTypeId] = issued.GetValueOrDefault(credentialTypeId) + count;

        return issued;
    }

    private static async Task<int> GetUsedCountAsync(
        Guid credentialTypeId,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, int> counts = await GetUsedCountsAsync([credentialTypeId], db, cancellationToken);
        return counts.GetValueOrDefault(credentialTypeId);
    }

    private static async Task<Dictionary<Guid, CredentialProvisioningTransactionResponse[]>> GetProvisioningAsync(
        Guid[] credentialIds,
        CredentialManagementDbContext db,
        CancellationToken cancellationToken)
    {
        CredentialProvisioningTransaction[] transactions = await db.CredentialProvisioningTransactions
            .AsNoTracking()
            .Where(transaction => credentialIds.Contains(transaction.CredentialId))
            .OrderBy(transaction => transaction.ScheduledFor)
            .ToArrayAsync(cancellationToken);

        return transactions
            .GroupBy(transaction => transaction.CredentialId)
            .ToDictionary(group => group.Key, group => group.Select(transaction => transaction.ToResponse()).ToArray());
    }

    private static IResult ToResult((int statusCode, ProblemDetails problemDetails) error) =>
        Results.Json(error.problemDetails, statusCode: error.statusCode);

    private static (int statusCode, ProblemDetails problemDetails) MapError(CredentialManagementErrors error) =>
        error switch
        {
            CredentialManagementErrors.CredentialTypeNotFound => Problem(StatusCodes.Status404NotFound, "Credential type not found."),
            CredentialManagementErrors.CredentialTypeTargetNotFound => Problem(StatusCodes.Status404NotFound, "Credential type target not found."),
            CredentialManagementErrors.CredentialReservationNotFound => Problem(StatusCodes.Status404NotFound, "Credential reservation not found."),
            CredentialManagementErrors.CredentialNotFound => Problem(StatusCodes.Status404NotFound, "Credential not found."),
            CredentialManagementErrors.CredentialTypeAlreadyExists => Problem(StatusCodes.Status409Conflict, "Credential type or target already exists."),
            CredentialManagementErrors.CredentialNumberUnavailable => Problem(StatusCodes.Status409Conflict, "Credential number is unavailable."),
            CredentialManagementErrors.CredentialTypeDisabled => Problem(StatusCodes.Status409Conflict, "Credential type is disabled."),
            CredentialManagementErrors.CredentialReservationExpired => Problem(StatusCodes.Status409Conflict, "Credential reservation is expired."),
            CredentialManagementErrors.CredentialReservationNotActive => Problem(StatusCodes.Status409Conflict, "Credential reservation is not active."),
            CredentialManagementErrors.CredentialReservationIdentityMismatch => Problem(StatusCodes.Status409Conflict, "Credential reservation belongs to a different identity."),
            CredentialManagementErrors.CredentialTypeRangeInvalid => Problem(StatusCodes.Status400BadRequest, "Credential type range is invalid."),
            CredentialManagementErrors.CredentialNumberOutsideRange => Problem(StatusCodes.Status400BadRequest, "Credential number is outside the credential type range."),
            CredentialManagementErrors.TemporaryCredentialRequiresValidUntil => Problem(StatusCodes.Status400BadRequest, "Temporary credentials require a valid until value."),
            CredentialManagementErrors.ValidUntilMustBeAfterValidFrom => Problem(StatusCodes.Status400BadRequest, "Valid until must be after valid from."),
            CredentialManagementErrors.ReasonRequired => Problem(StatusCodes.Status400BadRequest, "Reason text is required."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected credential management error.")
        };

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) =>
        (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
