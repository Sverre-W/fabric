using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Application;

public sealed record AccessPolicyChangeResult(
    AccessPolicy? Policy,
    IssuedResource? SatisfiedBy,
    SubjectSystemAccessState AccessState);

public enum AccessPolicyReconciliationFailureBehavior
{
    KeepPolicyForRetry,
    FailAndRetractPolicy
}

public sealed record CreateAccessPolicyOptions(
    AccessPolicyReconciliationFailureBehavior ReconciliationFailureBehavior = AccessPolicyReconciliationFailureBehavior.KeepPolicyForRetry);

public class AccessPolicyService(
    AccessPoliciesDbContext db,
    TimeProvider timeProvider,
    UnipassAccessPolicyReconciler unipassReconciler,
    AccessPolicyReconciliationTrigger reconciliationTrigger,
    ITenantContext tenantContext)
{
    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateCredentialPolicy(
        Guid systemId,
        Subject subject,
        Guid badgeTypeId,
        int? badgeNumber,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        DateTimeOffset? provisionFrom = null,
        CancellationToken cancellationToken = default) =>
        await CreateCredentialPolicy(
            systemId,
            subject,
            badgeTypeId,
            badgeNumber,
            effectiveFrom,
            effectiveUntil,
            provisionFrom,
            null,
            cancellationToken);

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateCredentialPolicy(
        Guid systemId,
        Subject subject,
        Guid badgeTypeId,
        int? badgeNumber,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        DateTimeOffset? provisionFrom,
        CreateAccessPolicyOptions? options,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicy, AccessPolicyErrors> create = await CreateCredentialPolicyEntity(
            systemId,
            subject,
            badgeTypeId,
            badgeNumber,
            provisionFrom ?? timeProvider.GetUtcNow(),
            effectiveFrom,
            effectiveUntil,
            cancellationToken);

        if (create.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(error);

        create.IsSuccess(out AccessPolicy policy);
        db.AccessPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);

        return await ReconcileAfterPolicyChange(policy, subject.Id, systemId, options ?? new CreateAccessPolicyOptions(), cancellationToken);
    }

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateCredentialPolicyAsync(
        Guid systemId,
        Subject subject,
        Guid badgeTypeId,
        int? badgeNumber,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        DateTimeOffset? provisionFrom = null,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicy, AccessPolicyErrors> create = await CreateCredentialPolicyEntity(
            systemId,
            subject,
            badgeTypeId,
            badgeNumber,
            provisionFrom ?? timeProvider.GetUtcNow(),
            effectiveFrom,
            effectiveUntil,
            cancellationToken);

        if (create.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(error);

        create.IsSuccess(out AccessPolicy policy);
        db.AccessPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);
        await EnqueueReconciliation(subject.Id, systemId, cancellationToken);

        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(policy, null, SubjectSystemAccessState.Empty(subject.Id, systemId)));
    }

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateAccessPolicy(
        Guid systemId,
        Subject subject,
        Guid accessLevelTypeId,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        DateTimeOffset? provisionFrom = null,
        CancellationToken cancellationToken = default) =>
        await CreateAccessPolicy(
            systemId,
            subject,
            accessLevelTypeId,
            effectiveFrom,
            effectiveUntil,
            provisionFrom,
            null,
            cancellationToken);

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateAccessPolicy(
        Guid systemId,
        Subject subject,
        Guid accessLevelTypeId,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        DateTimeOffset? provisionFrom,
        CreateAccessPolicyOptions? options,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicy, AccessPolicyErrors> create = await CreateAccessPolicyEntity(
            systemId,
            subject,
            accessLevelTypeId,
            provisionFrom ?? timeProvider.GetUtcNow(),
            effectiveFrom,
            effectiveUntil,
            cancellationToken);

        if (create.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(error);

        create.IsSuccess(out AccessPolicy policy);
        db.AccessPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);

        return await ReconcileAfterPolicyChange(policy, subject.Id, systemId, options ?? new CreateAccessPolicyOptions(), cancellationToken);
    }

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateAccessPolicyAsync(
        Guid systemId,
        Subject subject,
        Guid accessLevelTypeId,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        DateTimeOffset? provisionFrom = null,
        CancellationToken cancellationToken = default)
    {
        Result<AccessPolicy, AccessPolicyErrors> create = await CreateAccessPolicyEntity(
            systemId,
            subject,
            accessLevelTypeId,
            provisionFrom ?? timeProvider.GetUtcNow(),
            effectiveFrom,
            effectiveUntil,
            cancellationToken);

        if (create.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(error);

        create.IsSuccess(out AccessPolicy policy);
        db.AccessPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);
        await EnqueueReconciliation(subject.Id, systemId, cancellationToken);

        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(policy, null, SubjectSystemAccessState.Empty(subject.Id, systemId)));
    }

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> RetractPolicy(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        AccessPolicy? policy = await db.AccessPolicies
            .Include(accessPolicy => accessPolicy.Requirement)
            .SingleOrDefaultAsync(accessPolicy => accessPolicy.Id == policyId, cancellationToken);

        if (policy is null)
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(AccessPolicyErrors.PolicyNotFound);

        Guid subjectId = policy.Subject.Id;
        Guid systemId = policy.SystemId;

        db.AccessPolicies.Remove(policy);
        await db.SaveChangesAsync(cancellationToken);
        await EnqueueReconciliation(subjectId, systemId, cancellationToken);

        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(null, null, SubjectSystemAccessState.Empty(subjectId, systemId)));
    }

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> ReconcileSubjectSystemPolicies(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken = default)
    {
        Result<SubjectSystemAccessState, string> reconciliation = await ReconcileSubjectSystem(
            subjectId,
            systemId,
            cancellationToken);

        if (reconciliation.IsFailure(out string reason))
        {
            await MarkSubjectSystemPoliciesFailed(subjectId, systemId, reason, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
                new AccessPolicyChangeResult(null, null, SubjectSystemAccessState.Empty(subjectId, systemId)));
        }

        reconciliation.IsSuccess(out SubjectSystemAccessState state);
        await MarkSubjectSystemPoliciesReconciled(state, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(null, null, state));
    }

    public async Task<IReadOnlyList<AccessPolicyReconciliationWorkItem>> GetPendingReconciliationWorkItemsAsync(
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.AccessPolicies
            .AsNoTracking()
            .Where(policy => policy.ProvisionFrom <= now && policy.EffectiveUntil > now)
            .Where(policy => policy.ReconciliationStatus == ReconciliationStatus.PendingReconciliation
                             || policy.ReconciliationStatus == ReconciliationStatus.ReconciliationFailed)
            .Select(policy => new { policy.Subject.Id, policy.SystemId })
            .Distinct()
            .Select(policy => new AccessPolicyReconciliationWorkItem(tenantContext.TenantId, policy.Id, policy.SystemId))
            .ToListAsync(cancellationToken);
    }

    private async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> ReconcileAfterPolicyChange(
        AccessPolicy policy,
        Guid subjectId,
        Guid systemId,
        CreateAccessPolicyOptions options,
        CancellationToken cancellationToken)
    {
        Result<SubjectSystemAccessState, string> reconciliation = await ReconcileSubjectSystem(
            subjectId,
            systemId,
            cancellationToken);

        if (reconciliation.IsFailure(out string reason))
        {
            await MarkSubjectSystemPoliciesFailed(subjectId, systemId, reason, cancellationToken);
            if (options.ReconciliationFailureBehavior == AccessPolicyReconciliationFailureBehavior.FailAndRetractPolicy)
            {
                db.AccessPolicies.Remove(policy);
                await db.SaveChangesAsync(cancellationToken);
                return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(AccessPolicyErrors.ReconciliationFailed);
            }

            await db.SaveChangesAsync(cancellationToken);
            await EnqueueReconciliation(subjectId, systemId, cancellationToken);

            var failedState = SubjectSystemAccessState.Empty(subjectId, systemId);
            failedState.Satisfies(policy, out IssuedResource? failedResource);

            return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
                new AccessPolicyChangeResult(policy, failedResource, failedState));
        }

        reconciliation.IsSuccess(out SubjectSystemAccessState state);
        await MarkSubjectSystemPoliciesReconciled(state, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        state.Satisfies(policy, out IssuedResource? resource);
        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(policy, resource, state));
    }

    private async Task<Result<AccessPolicy, AccessPolicyErrors>> CreateCredentialPolicyEntity(
        Guid systemId,
        Subject subject,
        Guid badgeTypeId,
        int? badgeNumber,
        DateTimeOffset provisionFrom,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        CancellationToken cancellationToken)
    {
        if (!await SystemExists(systemId, cancellationToken))
            return Result.Failure<AccessPolicy, AccessPolicyErrors>(AccessPolicyErrors.SystemNotFound);

        BadgeType? badgeType = await db.BadgeTypes
            .SingleOrDefaultAsync(type => type.Id == badgeTypeId && type.SystemId == systemId, cancellationToken);

        if (badgeType is null)
            return Result.Failure<AccessPolicy, AccessPolicyErrors>(AccessPolicyErrors.BadgeTypeNotFound);

        return AccessPolicy.Create(
            systemId,
            subject,
            provisionFrom,
            effectiveFrom,
            effectiveUntil,
            CredentialRequirement.Create(badgeType, badgeNumber));
    }

    private async Task<Result<AccessPolicy, AccessPolicyErrors>> CreateAccessPolicyEntity(
        Guid systemId,
        Subject subject,
        Guid accessLevelTypeId,
        DateTimeOffset provisionFrom,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        CancellationToken cancellationToken)
    {
        if (!await SystemExists(systemId, cancellationToken))
            return Result.Failure<AccessPolicy, AccessPolicyErrors>(AccessPolicyErrors.SystemNotFound);

        AccessLevelType? accessLevel = await db.AccessLevelTypes
            .SingleOrDefaultAsync(type => type.Id == accessLevelTypeId && type.SystemId == systemId, cancellationToken);

        if (accessLevel is null)
            return Result.Failure<AccessPolicy, AccessPolicyErrors>(AccessPolicyErrors.AccessLevelTypeNotFound);

        return AccessPolicy.Create(
            systemId,
            subject,
            provisionFrom,
            effectiveFrom,
            effectiveUntil,
            AccessRequirement.Create(accessLevel));
    }

    private async Task EnqueueReconciliation(Guid subjectId, Guid systemId, CancellationToken cancellationToken)
    {
        await reconciliationTrigger.EnqueueAsync(
            new AccessPolicyReconciliationWorkItem(tenantContext.TenantId, subjectId, systemId),
            cancellationToken);
    }

    private async Task<bool> SystemExists(Guid systemId, CancellationToken cancellationToken) =>
        await db.AccessControlSystems.AnyAsync(system => system.Id == systemId, cancellationToken);

    private async Task MarkSubjectSystemPoliciesReconciled(
        SubjectSystemAccessState state,
        CancellationToken cancellationToken)
    {
        List<AccessPolicy> policies = await GetProvisionableSubjectSystemPolicies(state.SubjectId, state.SystemId, cancellationToken);

        foreach (AccessPolicy policy in policies)
        {
            state.Satisfies(policy, out IssuedResource? resource);
            policy.MarkReconciled(resource);
        }
    }

    private async Task MarkSubjectSystemPoliciesFailed(
        Guid subjectId,
        Guid systemId,
        string reason,
        CancellationToken cancellationToken)
    {
        List<AccessPolicy> policies = await GetProvisionableSubjectSystemPolicies(subjectId, systemId, cancellationToken);

        foreach (AccessPolicy policy in policies)
            policy.MarkReconciliationFailed(reason);
    }

    private async Task<List<AccessPolicy>> GetProvisionableSubjectSystemPolicies(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.AccessPolicies
            .Where(policy => policy.Subject.Id == subjectId)
            .Where(policy => policy.SystemId == systemId)
            .Where(policy => policy.ProvisionFrom <= now && policy.EffectiveUntil > now)
            .ToListAsync(cancellationToken);
    }

    private async Task<Result<SubjectSystemAccessState, string>> ReconcileSubjectSystem(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        AccessControlSystem? system = await db.AccessControlSystems
            .AsNoTracking()
            .SingleOrDefaultAsync(accessSystem => accessSystem.Id == systemId, cancellationToken);

        return system switch
        {
            null => Result.Failure<SubjectSystemAccessState, string>("System not found."),
            UnipassAccessControlSystem => await unipassReconciler.ReconcileSubjectSystem(subjectId, systemId, cancellationToken),
            LenelAccessControlSystem => Result.Success<SubjectSystemAccessState, string>(SubjectSystemAccessState.Empty(subjectId, systemId)),
            _ => Result.Failure<SubjectSystemAccessState, string>("System provider mismatch.")
        };
    }
}
