using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.AccessPolicies.Persistence;
using Fabric.Server.Core;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Application;

public sealed record AccessPolicyChangeResult(
    AccessPolicy? Policy,
    IssuedResource? SatisfiedBy,
    SubjectSystemAccessState AccessState);

public class AccessPolicyService(AccessPoliciesDbContext db, TimeProvider timeProvider)
{
    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateCredentialPolicy(
        Guid systemId,
        Subject subject,
        Guid badgeTypeId,
        int? badgeNumber,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        CancellationToken cancellationToken = default)
    {
        if (!await SystemExists(systemId, cancellationToken))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(AccessPolicyErrors.SystemNotFound);

        BadgeType? badgeType = await db.BadgeTypes
            .SingleOrDefaultAsync(type => type.Id == badgeTypeId && type.SystemId == systemId, cancellationToken);

        if (badgeType is null)
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(AccessPolicyErrors.BadgeTypeNotFound);

        var requirement = CredentialRequirement.Create(badgeType, badgeNumber);
        Result<AccessPolicy, AccessPolicyErrors> policyResult = AccessPolicy.Create(
            systemId,
            subject,
            effectiveFrom,
            effectiveUntil,
            requirement);

        if (policyResult.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(error);

        policyResult.IsSuccess(out AccessPolicy policy);
        db.AccessPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);

        return await ReconcileAfterPolicyChange(policy, subject.Id, systemId, cancellationToken);
    }

    public async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> CreateAccessPolicy(
        Guid systemId,
        Subject subject,
        Guid accessLevelTypeId,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        CancellationToken cancellationToken = default)
    {
        if (!await SystemExists(systemId, cancellationToken))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(AccessPolicyErrors.SystemNotFound);

        AccessLevelType? accessLevel = await db.AccessLevelTypes
            .SingleOrDefaultAsync(type => type.Id == accessLevelTypeId && type.SystemId == systemId, cancellationToken);

        if (accessLevel is null)
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(AccessPolicyErrors.AccessLevelTypeNotFound);

        var requirement = AccessRequirement.Create(accessLevel);
        Result<AccessPolicy, AccessPolicyErrors> policyResult = AccessPolicy.Create(
            systemId,
            subject,
            effectiveFrom,
            effectiveUntil,
            requirement);

        if (policyResult.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicyChangeResult, AccessPolicyErrors>(error);

        policyResult.IsSuccess(out AccessPolicy policy);
        db.AccessPolicies.Add(policy);
        await db.SaveChangesAsync(cancellationToken);

        return await ReconcileAfterPolicyChange(policy, subject.Id, systemId, cancellationToken);
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
        await MarkSubjectSystemPoliciesReconciled(subjectId, systemId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(null, null, state));
    }

    private async Task<Result<AccessPolicyChangeResult, AccessPolicyErrors>> ReconcileAfterPolicyChange(
        AccessPolicy policy,
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        Result<SubjectSystemAccessState, string> reconciliation = await ReconcileSubjectSystem(
            subjectId,
            systemId,
            cancellationToken);

        if (reconciliation.IsFailure(out string reason))
        {
            await MarkSubjectSystemPoliciesFailed(subjectId, systemId, reason, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var failedState = SubjectSystemAccessState.Empty(subjectId, systemId);
            failedState.Satisfies(policy, out IssuedResource? failedResource);

            return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
                new AccessPolicyChangeResult(policy, failedResource, failedState));
        }

        reconciliation.IsSuccess(out SubjectSystemAccessState state);
        await MarkSubjectSystemPoliciesReconciled(subjectId, systemId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        state.Satisfies(policy, out IssuedResource? resource);
        return Result.Success<AccessPolicyChangeResult, AccessPolicyErrors>(
            new AccessPolicyChangeResult(policy, resource, state));
    }

    private async Task<bool> SystemExists(Guid systemId, CancellationToken cancellationToken) =>
        await db.AccessControlSystems.AnyAsync(system => system.Id == systemId, cancellationToken);

    private async Task MarkSubjectSystemPoliciesReconciled(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        List<AccessPolicy> policies = await GetActiveSubjectSystemPolicies(subjectId, systemId, cancellationToken);

        foreach (AccessPolicy policy in policies)
            policy.MarkReconciled();
    }

    private async Task MarkSubjectSystemPoliciesFailed(
        Guid subjectId,
        Guid systemId,
        string reason,
        CancellationToken cancellationToken)
    {
        List<AccessPolicy> policies = await GetActiveSubjectSystemPolicies(subjectId, systemId, cancellationToken);

        foreach (AccessPolicy policy in policies)
            policy.MarkReconciliationFailed(reason);
    }

    private async Task<List<AccessPolicy>> GetActiveSubjectSystemPolicies(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.AccessPolicies
            .Where(policy => policy.Subject.Id == subjectId)
            .Where(policy => policy.SystemId == systemId)
            .Where(policy => policy.EffectiveFrom <= now && policy.EffectiveUntil > now)
            .ToListAsync(cancellationToken);
    }

    private static Task<Result<SubjectSystemAccessState, string>> ReconcileSubjectSystem(
        Guid subjectId,
        Guid systemId,
        CancellationToken cancellationToken)
    {
        // TODO: Load active policies for subject/system, fetch provider access state,
        // normalize to local BadgeTypeId/AccessLevelTypeId, apply missing changes,
        // then fetch and return final access state.
        return Task.FromResult(Result.Success<SubjectSystemAccessState, string>(
            SubjectSystemAccessState.Empty(subjectId, systemId)));
    }
}
