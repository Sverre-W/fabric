using AccessControl.Unipass.ChangeSets;
using AccessControl.Unipass.Contracts;
using AccessControl.Unipass.Entities;
using AccessControl.Unipass.Filters;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Application;

public sealed class UnipassAccessPolicyReconciler(
    AccessPoliciesDbContext db,
    UnipassApiFactory apiFactory,
    BadgeNumberAllocationService badgeNumbers,
    TimeProvider timeProvider)
{
    // Unipass stores access-rule times with inclusive-minute quirks; compare with skew, keep domain times exact.
    private static readonly TimeSpan AccessRuleSkew = TimeSpan.FromMinutes(2);

    public async Task<Result<SubjectSystemAccessState, string>> ReconcileSubjectSystem(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        UnipassAccessControlSystem? system = await db.AccessControlSystems
            .OfType<UnipassAccessControlSystem>()
            .Include(accessSystem => accessSystem.BadgeTypes)
            .Include(accessSystem => accessSystem.AccessLevels)
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        if (system is null)
            return Result.Failure<SubjectSystemAccessState, string>("Unipass system not found.");

        List<AccessPolicy> policies = await GetActivePolicies(subjectId, systemId, cancellationToken);
        if (policies.Count == 0)
            return Result.Success<SubjectSystemAccessState, string>(SubjectSystemAccessState.Empty(subjectId, systemId));

        Subject subject = policies[0].Subject;
        using IUnipassApi api = apiFactory.Create(system.Config);

        int personId;
        try
        {
            personId = await GetOrCreatePerson(api, subject, systemId, cancellationToken);
            UnipassProviderState state = await FetchState(api, subjectId, systemId, personId, system, cancellationToken);

            foreach (AccessPolicy policy in policies)
            {
                if (state.AccessState.Satisfies(policy, out IssuedResource? resource))
                {
                    await ReserveSatisfiedCredential(policy, resource, cancellationToken);
                    continue;
                }

                await ApplyMissingRequirement(api, personId, policy, system, cancellationToken);
                state = await FetchState(api, subjectId, systemId, personId, system, cancellationToken);
            }

            UnipassProviderState finalState = await FetchState(api, subjectId, systemId, personId, system, cancellationToken);
            return Result.Success<SubjectSystemAccessState, string>(finalState.AccessState);
        }
        catch (Exception ex)
        {
            return Result.Failure<SubjectSystemAccessState, string>(ex.Message);
        }
    }

    public async Task<Result<string>> RevokePolicyResources(AccessPolicy policy, CancellationToken cancellationToken)
    {
        UnipassAccessControlSystem? system = await db.AccessControlSystems
            .OfType<UnipassAccessControlSystem>()
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == policy.SystemId, cancellationToken);

        if (system is null)
            return Result.Success<string>();

        List<IssuedProviderResource> resources = await db.IssuedProviderResources
            .Where(resource => resource.PolicyId == policy.Id)
            .ToListAsync(cancellationToken);

        if (resources.Count == 0)
            return Result.Success<string>();

        using IUnipassApi api = apiFactory.Create(system.Config);

        try
        {
            foreach (IssuedProviderResource resource in resources)
                await RevokeResource(api, resource, cancellationToken);

            foreach (IssuedProviderResource resource in resources.Where(resource => resource.ResourceKind == ProviderResourceKind.Credential))
            {
                if (resource.BadgeTypeId.HasValue && resource.BadgeNumber.HasValue)
                    await badgeNumbers.ReleaseBadgeNumber(resource.SystemId, resource.BadgeTypeId.Value, resource.BadgeNumber.Value, cancellationToken);
            }

            db.IssuedProviderResources.RemoveRange(resources);
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success<string>();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private async Task<int> GetOrCreatePerson(
        IUnipassApi api,
        Subject subject,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        IdentityMapping? mapping = await db.IdentityMappings
            .SingleOrDefaultAsync(item => item.SubjectId == subject.Id && item.SystemId == systemId, cancellationToken);

        if (mapping is not null)
            return int.Parse(mapping.ExternalId);

        PersonChangeSet changeSet = PersonChangeSet.Create()
            .FirstName(subject.FirstName)
            .LastName(subject.LastName)
            .PersonType(subject.SubjectType == SubjectType.Visitor ? UnipassPersonType.Visitor : UnipassPersonType.Staff);

        UnipassOperationResponse response = await api.ApplyChangeSet(changeSet, cancellationToken);
        if (!response.Success || string.IsNullOrWhiteSpace(response.Id))
            throw new InvalidOperationException(response.Message ?? "Unipass person creation failed.");

        db.IdentityMappings.Add(IdentityMapping.Create(subject, systemId, response.Id));
        await db.SaveChangesAsync(cancellationToken);
        return int.Parse(response.Id);
    }

    private async Task<UnipassProviderState> FetchState(
        IUnipassApi api,
        Guid subjectId,
        Guid systemId,
        int personId,
        UnipassAccessControlSystem system,
        CancellationToken cancellationToken)
    {
        UnipassPerson person = await api.GetPerson(personId, cancellationToken)
            ?? throw new InvalidOperationException($"Unipass person {personId} not found.");
        List<UnipassAssignedAccessRule> assignedRules = await api.GetAssignedAccessRules(personId, cancellationToken);

        List<UnipassCredentialState> credentials = [.. person.Cards.Select(card => new UnipassCredentialState(card.Id, card.BadgeNumber))];
        List<UnipassAccessRuleState> accessRules = [.. assignedRules
            .Where(IsCurrentlyEffective)
            .Select(rule => new UnipassAccessRuleState(rule.Id, rule.SiteId, rule.RuleId))];

        List<IssuedResource> resources = [];
        foreach (UnipassCredentialState card in credentials)
        {
            UnipassBadgeType? badgeType = system.BadgeTypes.SingleOrDefault(type =>
                card.BadgeNumber >= type.Range.Start && card.BadgeNumber <= type.Range.Stop);
            if (badgeType is not null)
                resources.Add(UnipassCredential.Create(subjectId, badgeType.Id, systemId, card.BadgeNumber.ToString()));
        }

        foreach (UnipassAccessRuleState rule in accessRules)
        {
            UnipassAccessLevelType? accessLevel = system.AccessLevels.SingleOrDefault(type =>
                type.SiteId == rule.SiteId && type.AccessRuleId == rule.RuleId);
            if (accessLevel is not null)
                resources.Add(UnipassAccessLevel.Create(subjectId, accessLevel.Id, systemId));
        }

        return new UnipassProviderState(
            new SubjectSystemAccessState(subjectId, systemId, resources),
            credentials,
            accessRules);
    }

    private async Task ApplyMissingRequirement(
        IUnipassApi api,
        int personId,
        AccessPolicy policy,
        UnipassAccessControlSystem system,
        CancellationToken cancellationToken)
    {
        switch (policy.Requirement)
        {
            case CredentialRequirement credential:
                await AssignCredential(api, personId, policy, credential, system, cancellationToken);
                break;
            case AccessRequirement access:
                await AssignAccessLevel(api, personId, policy, access, cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unknown policy requirement.");
        }
    }

    private async Task ReserveSatisfiedCredential(
        AccessPolicy policy,
        IssuedResource? resource,
        CancellationToken cancellationToken)
    {
        if (policy.Requirement is not CredentialRequirement credential ||
            credential.BadgeType is not UnipassBadgeType badgeType ||
            resource is not UnipassCredential unipassCredential ||
            !int.TryParse(unipassCredential.BadgeNumber, out int badgeNumber))
        {
            return;
        }

        await badgeNumbers.TakeBadgeNumber(policy.SystemId, badgeType.Id, policy.Subject.Id, badgeNumber, cancellationToken);
    }

    private async Task AssignCredential(
        IUnipassApi api,
        int personId,
        AccessPolicy policy,
        CredentialRequirement credential,
        UnipassAccessControlSystem system,
        CancellationToken cancellationToken)
    {
        if (credential.BadgeType is not UnipassBadgeType badgeType)
            throw new InvalidOperationException("Badge type is not Unipass.");

        int badgeNumber;
        if (credential.BadgeNumber.HasValue)
        {
            badgeNumber = credential.BadgeNumber.Value;
            if (badgeNumber < badgeType.Range.Start || badgeNumber > badgeType.Range.Stop)
                throw new InvalidOperationException("Badge number is outside badge type range.");

            if (!await badgeNumbers.TakeBadgeNumber(system.Id, badgeType.Id, policy.Subject.Id, badgeNumber, cancellationToken))
                throw new InvalidOperationException("Badge number is already used by Fabric.");
        }
        else
        {
            int? allocated = await badgeNumbers.TakeNextBadgeNumber(system.Id, badgeType.Id, policy.Subject.Id, badgeType.Range, cancellationToken);
            badgeNumber = allocated ?? throw new InvalidOperationException("No badge numbers available.");
        }

        try
        {
            UnipassOperationResponse response = await api.ApplyChangeSet(CardChangeSet.Assign(personId, badgeNumber), cancellationToken);
            if (!response.Success || string.IsNullOrWhiteSpace(response.Id))
                throw new InvalidOperationException(response.Message ?? "Unipass card assignment failed.");

            db.IssuedProviderResources.Add(IssuedProviderResource.CreateCredential(
                policy.Id,
                policy.Subject.Id,
                system.Id,
                badgeType.Id,
                badgeNumber,
                personId.ToString(),
                response.Id));
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await badgeNumbers.ReleaseBadgeNumber(system.Id, badgeType.Id, badgeNumber, cancellationToken);
            throw;
        }
    }

    private async Task AssignAccessLevel(
        IUnipassApi api,
        int personId,
        AccessPolicy policy,
        AccessRequirement access,
        CancellationToken cancellationToken)
    {
        if (access.AccessLevel is not UnipassAccessLevelType accessLevel)
            throw new InvalidOperationException("Access level type is not Unipass.");

        await api.ApplyChangeSet(PersonChangeSet.Update(personId).EnableSite(accessLevel.SiteId), cancellationToken);
        UnipassOperationResponse response = await api.ApplyChangeSet(
            AssignedAccessRuleChangeSet.Assign(personId, accessLevel.SiteId, accessLevel.AccessRuleId)
                .StartTime(policy.EffectiveFrom)
                .EndTime(policy.EffectiveUntil),
            cancellationToken);

        if (!response.Success || string.IsNullOrWhiteSpace(response.Id))
            throw new InvalidOperationException(response.Message ?? "Unipass access rule assignment failed.");

        db.IssuedProviderResources.Add(IssuedProviderResource.CreateAccessLevel(
            policy.Id,
            policy.Subject.Id,
            policy.SystemId,
            accessLevel.Id,
            personId.ToString(),
            response.Id));
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeResource(IUnipassApi api, IssuedProviderResource resource, CancellationToken cancellationToken)
    {
        int personId = int.Parse(resource.ExternalPersonId);
        int externalResourceId = int.Parse(resource.ExternalResourceId);

        switch (resource.ResourceKind)
        {
            case ProviderResourceKind.Credential:
                await api.ApplyChangeSet(CardChangeSet.Revoke(personId, externalResourceId), cancellationToken);
                break;
            case ProviderResourceKind.AccessLevel:
                if (!resource.AccessLevelTypeId.HasValue)
                    throw new InvalidOperationException("Tracked access level resource is missing access level type id.");

                UnipassAccessLevelType accessLevel = await db.AccessLevelTypes
                    .OfType<UnipassAccessLevelType>()
                    .SingleAsync(type => type.Id == resource.AccessLevelTypeId.Value, cancellationToken);
                await api.ApplyChangeSet(AssignedAccessRuleChangeSet.Revoke(personId, accessLevel.SiteId, externalResourceId), cancellationToken);
                break;
            default:
                throw new InvalidOperationException("Unknown tracked resource kind.");
        }
    }

    private async Task<List<AccessPolicy>> GetActivePolicies(Guid subjectId, Guid systemId, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.AccessPolicies
            .Include(policy => policy.Requirement)
            .Include(policy => ((CredentialRequirement)policy.Requirement).BadgeType)
            .Include(policy => ((AccessRequirement)policy.Requirement).AccessLevel)
            .Where(policy => policy.Subject.Id == subjectId)
            .Where(policy => policy.SystemId == systemId)
            .Where(policy => policy.EffectiveFrom <= now && policy.EffectiveUntil > now)
            .ToListAsync(cancellationToken);
    }

    private bool IsCurrentlyEffective(UnipassAssignedAccessRule rule)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        return (rule.StartDate is null || rule.StartDate <= now.Add(AccessRuleSkew)) &&
               (rule.EndDate is null || rule.EndDate > now.Subtract(AccessRuleSkew));
    }

    private sealed record UnipassProviderState(
        SubjectSystemAccessState AccessState,
        IReadOnlyList<UnipassCredentialState> Credentials,
        IReadOnlyList<UnipassAccessRuleState> AccessRules);

    private sealed record UnipassCredentialState(int CardId, int BadgeNumber);

    private sealed record UnipassAccessRuleState(int AssignedRuleId, int SiteId, int RuleId);
}
