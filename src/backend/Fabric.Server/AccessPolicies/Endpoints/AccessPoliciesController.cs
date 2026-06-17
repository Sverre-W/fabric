using Fabric.Server.AccessPolicies.Application;
using Fabric.Server.AccessPolicies.Contracts;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Endpoints;

[ApiController]
public class AccessPoliciesController
{
    [HttpGet("/api/access-policies")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<AccessPolicyResponse>))]
    [EndpointDescription("List access policies")]
    [EndpointSummary("List access policies")]
    public async Task<IResult> ListAccessPolicies(
        [FromQuery] ListAccessPoliciesRequest request,
        [FromQuery] Guid[]? ids,
        [FromServices] AccessPoliciesDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AccessPolicy> query = AccessPoliciesWithRequirements(db).AsNoTracking();

        if (ids is { Length: > 0 })
            query = query.Where(policy => ids.Contains(policy.Id));

        if (request.SystemId.HasValue)
            query = query.Where(policy => policy.SystemId == request.SystemId.Value);

        if (request.SubjectId.HasValue)
            query = query.Where(policy => policy.Subject.Id == request.SubjectId.Value);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            string name = request.Name.ToLower();
            query = query.Where(policy =>
                policy.Subject.FirstName.ToLower().Contains(name) ||
                policy.Subject.LastName.ToLower().Contains(name));
        }

        IPaged<AccessPolicy> result = await query
            .OrderBy(policy => policy.Subject.LastName)
            .ThenBy(policy => policy.Subject.FirstName)
            .ThenBy(policy => policy.EffectiveFrom)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(result.Map(policy => policy.ToResponse()));
    }

    [HttpPost("/api/access-policies/credentials")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessPolicyChangeResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Create a credential access policy")]
    [EndpointSummary("Create credential policy")]
    public async Task<IResult> CreateCredentialPolicy(
        [FromBody] CreateCredentialPolicyRequest request,
        [FromServices] AccessPolicyService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicyChangeResult, AccessPolicyErrors> result = await service.CreateCredentialPolicy(
            request.SystemId,
            request.Subject.ToDomain(),
            request.BadgeTypeId,
            request.BadgeNumber,
            request.EffectiveFrom,
            request.EffectiveUntil,
            cancellationToken);

        return result.Map(change => change.ToResponse()).AsResponse(MapError);
    }

    [HttpPost("/api/access-policies/access-levels")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessPolicyChangeResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Create an access-level policy")]
    [EndpointSummary("Create access-level policy")]
    public async Task<IResult> CreateAccessPolicy(
        [FromBody] CreateAccessPolicyRequest request,
        [FromServices] AccessPolicyService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicyChangeResult, AccessPolicyErrors> result = await service.CreateAccessPolicy(
            request.SystemId,
            request.Subject.ToDomain(),
            request.AccessLevelTypeId,
            request.EffectiveFrom,
            request.EffectiveUntil,
            cancellationToken);

        return result.Map(change => change.ToResponse()).AsResponse(MapError);
    }

    [HttpPost("/api/access-policies/{policyId:guid}/retract")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessPolicyChangeResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [EndpointDescription("Retract an access policy and reconcile subject access")]
    [EndpointSummary("Retract access policy")]
    public async Task<IResult> RetractPolicy(
        Guid policyId,
        [FromServices] AccessPolicyService service,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicyChangeResult, AccessPolicyErrors> result = await service.RetractPolicy(policyId, cancellationToken);
        return result.Map(change => change.ToResponse()).AsResponse(MapError);
    }

    private static IQueryable<AccessPolicy> AccessPoliciesWithRequirements(AccessPoliciesDbContext db) =>
        db.AccessPolicies
            .Include(policy => policy.Requirement)
            .Include(policy => ((CredentialRequirement)policy.Requirement).BadgeType)
            .Include(policy => ((AccessRequirement)policy.Requirement).AccessLevel);

    private static (int statusCode, ProblemDetails? problemDetails) MapError(AccessPolicyErrors error) =>
        error switch
        {
            AccessPolicyErrors.SystemNotFound => Problem(StatusCodes.Status404NotFound, "System not found."),
            AccessPolicyErrors.PolicyNotFound => Problem(StatusCodes.Status404NotFound, "Policy not found."),
            AccessPolicyErrors.BadgeTypeNotFound => Problem(StatusCodes.Status404NotFound, "Badge type not found."),
            AccessPolicyErrors.AccessLevelTypeNotFound => Problem(StatusCodes.Status404NotFound, "Access level type not found."),
            AccessPolicyErrors.EffectiveFromMustBeBeforeEffectiveUntil => Problem(StatusCodes.Status400BadRequest, "Effective from must be before effective until."),
            AccessPolicyErrors.RequirementDoesNotBelongToSystem => Problem(StatusCodes.Status400BadRequest, "Requirement does not belong to system."),
            AccessPolicyErrors.ReconciliationFailureReasonRequired => Problem(StatusCodes.Status400BadRequest, "Reconciliation failure reason required."),
            AccessPolicyErrors.ReconciliationFailed => Problem(StatusCodes.Status409Conflict, "Reconciliation failed."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected access policy error.")
        };

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) =>
        (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
