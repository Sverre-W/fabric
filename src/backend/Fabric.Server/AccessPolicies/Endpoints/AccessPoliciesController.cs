using Fabric.Server.AccessPolicies.Application;
using Fabric.Server.AccessPolicies.Contracts;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Endpoints;

public static class AccessPolicyEndpoints
{
    public static IEndpointRouteBuilder MapAccessPolicyEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder policies = app.MapGroup("/api/access-policies/policies");

        policies.MapGet("", ListAccessPolicies)
            .WithDescription("List access policies")
            .WithSummary("List access policies")
            .Produces<Page<AccessPolicyResponse>>();
        policies.MapPost("/credentials", CreateCredentialPolicy)
            .WithDescription("Create a credential access policy")
            .WithSummary("Create credential policy")
            .Produces<AccessPolicyChangeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        policies.MapPost("/access-levels", CreateAccessPolicy)
            .WithDescription("Create an access-level policy")
            .WithSummary("Create access-level policy")
            .Produces<AccessPolicyChangeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        policies.MapPost("/{policyId:guid}/retract", RetractPolicy)
            .WithDescription("Retract an access policy and reconcile subject access")
            .WithSummary("Retract access policy")
            .Produces<AccessPolicyChangeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListAccessPolicies(
        [AsParameters] ListAccessPoliciesRequest request,
        [FromQuery] Guid[]? ids,
        AccessPoliciesDbContext db,
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

    private static async Task<IResult> CreateCredentialPolicy(
        [FromBody] CreateCredentialPolicyRequest request,
        AccessPolicyService service,
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

    private static async Task<IResult> CreateAccessPolicy(
        [FromBody] CreateAccessPolicyRequest request,
        AccessPolicyService service,
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

    private static async Task<IResult> RetractPolicy(
        Guid policyId,
        AccessPolicyService service,
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
