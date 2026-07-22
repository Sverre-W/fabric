using Fabric.Server.AccessCatalog.Domain;
using Fabric.Server.AccessCatalog.Persistence;
using Fabric.Server.Core;
using Fabric.Server.Locations.Application;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessCatalog.Application;

public sealed class ApprovalDecisionService(
    AccessCatalogDbContext db,
    AccessGrantService accessGrantService,
    LocationService locationService,
    TimeProvider timeProvider)
{
    public async Task<Result<ApprovalDecision, AccessCatalogErrors>> DecideAsync(
        Guid approvalRequirementId,
        Guid approverIdentityId,
        ApprovalDecisionKind decisionKind,
        string? note,
        CancellationToken cancellationToken = default)
    {
        ApprovalRequirement? requirement = await db.ApprovalRequirements.SingleOrDefaultAsync(item => item.Id == approvalRequirementId, cancellationToken);
        if (requirement is null)
            return Result.Failure<ApprovalDecision, AccessCatalogErrors>(AccessCatalogErrors.ApprovalRequirementNotFound);

        if (requirement.Status != ApprovalStatus.Pending)
            return Result.Failure<ApprovalDecision, AccessCatalogErrors>(AccessCatalogErrors.ApprovalRequirementAlreadyCompleted);

        PackageRequest? request = await db.PackageRequests.SingleOrDefaultAsync(item => item.Id == requirement.RequestId, cancellationToken);
        if (request is null)
            return Result.Failure<ApprovalDecision, AccessCatalogErrors>(AccessCatalogErrors.PackageRequestNotFound);

        if (request.Status != PackageRequestStatus.PendingApproval)
            return Result.Failure<ApprovalDecision, AccessCatalogErrors>(AccessCatalogErrors.ApprovalDecisionNotAllowed);

        if (!await CanApproveAsync(requirement, approverIdentityId, cancellationToken))
            return Result.Failure<ApprovalDecision, AccessCatalogErrors>(AccessCatalogErrors.ApprovalDecisionNotAllowed);

        DateTimeOffset now = timeProvider.GetUtcNow();
        ApprovalDecision decision = ApprovalDecision.Create(
            request.Id,
            requirement.Id,
            approverIdentityId,
            requirement.Role,
            decisionKind,
            note,
            now);

        db.ApprovalDecisions.Add(decision);

        switch (decisionKind)
        {
            case ApprovalDecisionKind.Approve:
                requirement.MarkApproved(now);
                break;
            case ApprovalDecisionKind.Reject:
                requirement.MarkRejected(now);
                request.MarkRejected(now);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success<ApprovalDecision, AccessCatalogErrors>(decision);
        }

        List<ApprovalRequirement> requirements = await db.ApprovalRequirements
            .Where(item => item.RequestId == request.Id)
            .ToListAsync(cancellationToken);

        bool allCompleted = requirements.All(item => item.Status == ApprovalStatus.Approved || item.Status == ApprovalStatus.SystemApproved);

        if (allCompleted)
        {
            Guid[] locationIds = await db.PackageRequestLocations
                .Where(item => item.RequestId == request.Id)
                .Select(item => item.LocationId)
                .ToArrayAsync(cancellationToken);

            Result<AccessGrant, AccessCatalogErrors> grantResult = await accessGrantService.CreateAsync(
                request.PackageId,
                request.BeneficiaryIdentityId,
                locationIds,
                AssignmentChannel.CatalogRequest,
                AssignmentSourceKind.CatalogRequest,
                request.Id,
                request.DurationKind,
                request.ValidFrom,
                request.ValidUntil,
                request.RequestReason,
                cancellationToken);

            if (grantResult.IsFailure(out AccessCatalogErrors grantError))
                return Result.Failure<ApprovalDecision, AccessCatalogErrors>(grantError);

            request.MarkApproved(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<ApprovalDecision, AccessCatalogErrors>(decision);
    }

    private async Task<bool> CanApproveAsync(ApprovalRequirement requirement, Guid approverIdentityId, CancellationToken cancellationToken)
    {
        return requirement.Type switch
        {
            ApprovalRequirementType.Organizational => requirement.RequiredApproverIdentityId == approverIdentityId,
            ApprovalRequirementType.Destination when requirement.ApprovalGroupId.HasValue => await IsDestinationApproverAsync(requirement, approverIdentityId, cancellationToken),
            _ => false
        };
    }

    private async Task<bool> IsDestinationApproverAsync(ApprovalRequirement requirement, Guid approverIdentityId, CancellationToken cancellationToken)
    {
        ApprovalGroupMember[] members = await db.ApprovalGroupMembers
            .Where(item => item.ApprovalGroupId == requirement.ApprovalGroupId!.Value)
            .Where(item => item.IdentityId == approverIdentityId)
            .ToArrayAsync(cancellationToken);

        foreach (ApprovalGroupMember member in members)
        {
            if (await locationService.IsPartOfLocationTree(requirement.LocationId, member.ResponsibleLocationId, cancellationToken))
                return true;
        }

        return false;
    }
}
