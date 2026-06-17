using Fabric.Server.Core;

namespace Fabric.Server.AccessPolicies.Domain;

public sealed class AccessPolicy
{
    private AccessPolicy() { }

    private AccessPolicy(
        Guid systemId,
        Subject subject,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        PolicyRequirement requirement)
    {
        Id = Guid.NewGuid();
        SystemId = systemId;
        Subject = subject;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        Requirement = requirement;
        ReconciliationStatus = ReconciliationStatus.PendingReconciliation;
    }

    public Guid Id { get; private set; }
    public Guid SystemId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset EffectiveUntil { get; private set; }
    public PolicyRequirement Requirement { get; private set; } = null!;
    public ReconciliationStatus ReconciliationStatus { get; private set; }
    public string? ReconciliationFailureReason { get; private set; }

    public static Result<AccessPolicy, AccessPolicyErrors> Create(
        Guid systemId,
        Subject subject,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        PolicyRequirement requirement)
    {
        Result<AccessPolicyErrors> validation = ValidateEffectiveRange(effectiveFrom, effectiveUntil);
        if (validation.IsFailure(out AccessPolicyErrors error))
            return Result.Failure<AccessPolicy, AccessPolicyErrors>(error);

        if (!requirement.BelongsToSystem(systemId))
            return Result.Failure<AccessPolicy, AccessPolicyErrors>(AccessPolicyErrors.RequirementDoesNotBelongToSystem);

        return Result.Success<AccessPolicy, AccessPolicyErrors>(
            new AccessPolicy(systemId, subject, effectiveFrom, effectiveUntil, requirement));
    }

    public bool IsExpired(DateTimeOffset now) => now >= EffectiveUntil;

    public void MarkPendingReconciliation()
    {
        ReconciliationStatus = ReconciliationStatus.PendingReconciliation;
        ReconciliationFailureReason = null;
    }

    public void MarkReconciled()
    {
        ReconciliationStatus = ReconciliationStatus.Reconciled;
        ReconciliationFailureReason = null;
    }

    public Result<AccessPolicyErrors> MarkReconciliationFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(AccessPolicyErrors.ReconciliationFailureReasonRequired);

        ReconciliationStatus = ReconciliationStatus.ReconciliationFailed;
        ReconciliationFailureReason = reason;
        return Result.Success<AccessPolicyErrors>();
    }

    private static Result<AccessPolicyErrors> ValidateEffectiveRange(
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil)
    {
        return effectiveFrom < effectiveUntil
            ? Result.Success<AccessPolicyErrors>()
            : Result.Failure(AccessPolicyErrors.EffectiveFromMustBeBeforeEffectiveUntil);
    }
}
