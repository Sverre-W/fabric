using Fabric.Server.AccessCatalog.Domain;
using Fabric.Server.AccessCatalog.Persistence;
using Fabric.Server.Core;
using Fabric.Server.Employees.Domain;
using Fabric.Server.Employees.Persistence;
using Fabric.Server.Identities.Persistence;
using Fabric.Server.Locations.Application;
using Fabric.Server.Locations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessCatalog.Application;

public sealed class PackageRequestService(
    AccessCatalogDbContext db,
    AccessGrantService accessGrantService,
    EmployeesDbContext employeesDb,
    IdentitiesDbContext identitiesDb,
    LocationsDbContext locationsDb,
    LocationService locationService,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan ApprovalWindow = TimeSpan.FromDays(7);

    public async Task<Result<PackageRequest, AccessCatalogErrors>> CreateAsync(
        Guid packageId,
        Guid requesterIdentityId,
        Guid beneficiaryIdentityId,
        Guid[] locationIds,
        string requestReason,
        AccessDurationKind durationKind,
        DateTimeOffset validFrom,
        DateTimeOffset? validUntil,
        CancellationToken cancellationToken = default)
    {
        if (locationIds.Length == 0)
            return Result.Failure<PackageRequest, AccessCatalogErrors>(AccessCatalogErrors.LocationRequired);

        Package? package = await db.Packages.SingleOrDefaultAsync(item => item.Id == packageId, cancellationToken);
        if (package is null)
            return Result.Failure<PackageRequest, AccessCatalogErrors>(AccessCatalogErrors.PackageNotFound);

        if (package.Status != PackageStatus.Active)
            return Result.Failure<PackageRequest, AccessCatalogErrors>(AccessCatalogErrors.PackageInactive);

        if (!await identitiesDb.Identities.AnyAsync(item => item.Id == requesterIdentityId, cancellationToken) ||
            !await identitiesDb.Identities.AnyAsync(item => item.Id == beneficiaryIdentityId, cancellationToken))
        {
            return Result.Failure<PackageRequest, AccessCatalogErrors>(AccessCatalogErrors.IdentityNotFound);
        }

        int locationCount = await locationsDb.LocationLookups.CountAsync(item => locationIds.Contains(item.Id), cancellationToken);
        if (locationCount != locationIds.Distinct().Count())
            return Result.Failure<PackageRequest, AccessCatalogErrors>(AccessCatalogErrors.LocationRequired);

        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<PackageRequest, AccessCatalogErrors> create = PackageRequest.Create(
            packageId,
            requesterIdentityId,
            beneficiaryIdentityId,
            requestReason,
            durationKind,
            validFrom,
            validUntil,
            now,
            now.Add(ApprovalWindow));
        if (create.IsFailure(out AccessCatalogErrors error))
            return Result.Failure<PackageRequest, AccessCatalogErrors>(error);

        create.IsSuccess(out PackageRequest request);
        db.PackageRequests.Add(request);

        Guid[] distinctLocationIds = locationIds.Distinct().ToArray();
        foreach (Guid locationId in distinctLocationIds)
            db.PackageRequestLocations.Add(PackageRequestLocation.Create(request.Id, locationId));

        Guid[] accessItemIds = await db.PackageAccessItems
            .Where(item => item.PackageId == packageId)
            .Select(item => item.AccessItemId)
            .ToArrayAsync(cancellationToken);

        if (accessItemIds.Length == 0)
            return Result.Failure<PackageRequest, AccessCatalogErrors>(AccessCatalogErrors.PackageMustContainAccessItems);

        List<ApprovalRequirement> requirements = await BuildRequirementsAsync(request, accessItemIds, distinctLocationIds, cancellationToken);
        db.ApprovalRequirements.AddRange(requirements);

        bool hasPending = requirements.Any(item => item.Status == ApprovalStatus.Pending);
        if (hasPending)
        {
            request.MarkPendingApproval();
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success<PackageRequest, AccessCatalogErrors>(request);
        }

        request.MarkApproved(now);

        Result<AccessGrant, AccessCatalogErrors> grant = await accessGrantService.CreateAsync(
            request.PackageId,
            request.BeneficiaryIdentityId,
            distinctLocationIds,
            AssignmentChannel.CatalogRequest,
            AssignmentSourceKind.CatalogRequest,
            request.Id,
            request.DurationKind,
            request.ValidFrom,
            request.ValidUntil,
            request.RequestReason,
            cancellationToken);

        if (grant.IsFailure(out AccessCatalogErrors grantError))
            return Result.Failure<PackageRequest, AccessCatalogErrors>(grantError);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<PackageRequest, AccessCatalogErrors>(request);
    }

    public async Task<IReadOnlyList<Guid>> GetExpirableRequestIdsAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        return await db.PackageRequests
            .AsNoTracking()
            .Where(item => item.Status == PackageRequestStatus.PendingApproval)
            .Where(item => item.ExpiresAt <= now)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExpireAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        PackageRequest? request = await db.PackageRequests
            .SingleOrDefaultAsync(item => item.Id == requestId && item.Status == PackageRequestStatus.PendingApproval && item.ExpiresAt <= now, cancellationToken);

        if (request is null)
            return false;

        request.MarkExpired(now);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<List<ApprovalRequirement>> BuildRequirementsAsync(
        PackageRequest request,
        Guid[] accessItemIds,
        Guid[] locationIds,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        List<ApprovalRequirement> requirements = [];

        Dictionary<Guid, ApprovalDefinition> definitions = await db.ApprovalDefinitions
            .Where(item => accessItemIds.Contains(item.AccessItemId))
            .ToDictionaryAsync(item => item.AccessItemId, cancellationToken);

        Employee? beneficiary = await employeesDb.Employees.SingleOrDefaultAsync(item => item.IdentityId == request.BeneficiaryIdentityId, cancellationToken);

        foreach (Guid locationId in locationIds)
        {
            foreach (Guid accessItemId in accessItemIds)
            {
                if (!definitions.TryGetValue(accessItemId, out ApprovalDefinition? definition))
                    continue;

                if (definition.DestinationApprovalGroupId.HasValue)
                {
                    bool hasApprover = await HasDestinationApproverAsync(definition.DestinationApprovalGroupId.Value, locationId, cancellationToken);
                    requirements.Add(hasApprover
                        ? ApprovalRequirement.CreateDestination(request.Id, accessItemId, locationId, definition.DestinationApprovalGroupId.Value, ApprovalDecisionRole.FacilityManager, now)
                        : ApprovalRequirement.CreateSystemApproved(request.Id, accessItemId, locationId, ApprovalRequirementType.Destination, ApprovalDecisionRole.FacilityManager, definition.DestinationApprovalGroupId.Value, "No approver configured for request location.", now));
                }

                if (definition.OrganizationalApprovalMode == OrganizationalApprovalMode.ManagerChain)
                {
                    for (int level = 1; level <= definition.OrganizationalApprovalLevels; level++)
                    {
                        Guid? approverIdentityId = await ResolveManagerIdentityIdAsync(beneficiary, level, cancellationToken);
                        ApprovalDecisionRole role = ToManagerRole(level);

                        requirements.Add(approverIdentityId.HasValue
                            ? ApprovalRequirement.CreateOrganizational(request.Id, accessItemId, locationId, approverIdentityId.Value, role, now)
                            : ApprovalRequirement.CreateSystemApproved(request.Id, accessItemId, locationId, ApprovalRequirementType.Organizational, role, null, $"No manager configured for organizational approval level L+{level}.", now));
                    }
                }
            }
        }

        return requirements;
    }

    private async Task<bool> HasDestinationApproverAsync(Guid approvalGroupId, Guid locationId, CancellationToken cancellationToken)
    {
        ApprovalGroupMember[] members = await db.ApprovalGroupMembers
            .Where(item => item.ApprovalGroupId == approvalGroupId)
            .ToArrayAsync(cancellationToken);

        foreach (ApprovalGroupMember member in members)
        {
            if (await locationService.IsPartOfLocationTree(locationId, member.ResponsibleLocationId, cancellationToken))
                return true;
        }

        return false;
    }

    private async Task<Guid?> ResolveManagerIdentityIdAsync(Employee? employee, int level, CancellationToken cancellationToken)
    {
        if (employee is null)
            return null;

        Guid? managerEmployeeId = employee.ManagerEmployeeId;
        for (int currentLevel = 1; currentLevel <= level; currentLevel++)
        {
            if (!managerEmployeeId.HasValue)
                return null;

            Employee? manager = await employeesDb.Employees.SingleOrDefaultAsync(item => item.Id == managerEmployeeId.Value, cancellationToken);
            if (manager is null)
                return null;

            if (currentLevel == level)
                return manager.IdentityId;

            managerEmployeeId = manager.ManagerEmployeeId;
        }

        return null;
    }

    private static ApprovalDecisionRole ToManagerRole(int level) => level switch
    {
        1 => ApprovalDecisionRole.L1,
        2 => ApprovalDecisionRole.L2,
        _ => ApprovalDecisionRole.L3
    };
}
