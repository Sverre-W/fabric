using Fabric.Server.Core;

namespace Fabric.Server.AccessPolicies.Domain;

public sealed class AccessPolicy
{
    private AccessPolicy() { }

    private AccessPolicy(
        Guid systemId,
        Subject subject,
        DateTimeOffset provisionFrom,
        DateTimeOffset effectiveFrom,
        DateTimeOffset effectiveUntil,
        PolicyRequirement requirement)
    {
        Id = Guid.NewGuid();
        SystemId = systemId;
        Subject = subject;
        ProvisionFrom = provisionFrom;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        Requirement = requirement;
        ReconciliationStatus = ReconciliationStatus.PendingReconciliation;
    }

    public Guid Id { get; private set; }
    public Guid SystemId { get; private set; }
    public Subject Subject { get; private set; } = null!;
    public DateTimeOffset ProvisionFrom { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset EffectiveUntil { get; private set; }
    public PolicyRequirement Requirement { get; private set; } = null!;
    public ReconciliationStatus ReconciliationStatus { get; private set; }
    public string? ReconciliationFailureReason { get; private set; }
    public ProviderResourceKind? SatisfiedByKind { get; private set; }
    public Guid? SatisfiedBySubjectId { get; private set; }
    public Guid? SatisfiedBySystemId { get; private set; }
    public Guid? SatisfiedByBadgeTypeId { get; private set; }
    public string? SatisfiedByBadgeNumber { get; private set; }
    public Guid? SatisfiedByAccessLevelTypeId { get; private set; }

    public IssuedResource? SatisfiedBy => SatisfiedByKind switch
    {
        ProviderResourceKind.Credential when SatisfiedBySubjectId.HasValue && SatisfiedBySystemId.HasValue && SatisfiedByBadgeTypeId.HasValue && !string.IsNullOrWhiteSpace(SatisfiedByBadgeNumber) =>
            UnipassCredential.Create(SatisfiedBySubjectId.Value, SatisfiedByBadgeTypeId.Value, SatisfiedBySystemId.Value, SatisfiedByBadgeNumber),
        ProviderResourceKind.AccessLevel when SatisfiedBySubjectId.HasValue && SatisfiedBySystemId.HasValue && SatisfiedByAccessLevelTypeId.HasValue =>
            UnipassAccessLevel.Create(SatisfiedBySubjectId.Value, SatisfiedByAccessLevelTypeId.Value, SatisfiedBySystemId.Value),
        _ => null
    };

    public static Result<AccessPolicy, AccessPolicyErrors> Create(
        Guid systemId,
        Subject subject,
        DateTimeOffset provisionFrom,
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
            new AccessPolicy(systemId, subject, provisionFrom, effectiveFrom, effectiveUntil, requirement));
    }

    public bool ShouldProvision(DateTimeOffset now) => ProvisionFrom <= now && EffectiveUntil > now;

    public bool IsExpired(DateTimeOffset now) => now >= EffectiveUntil;

    public void MarkPendingReconciliation()
    {
        ReconciliationStatus = ReconciliationStatus.PendingReconciliation;
        ReconciliationFailureReason = null;
        ClearSatisfiedBy();
    }

    public void MarkReconciled(IssuedResource? satisfiedBy)
    {
        ReconciliationStatus = ReconciliationStatus.Reconciled;
        ReconciliationFailureReason = null;

        switch (satisfiedBy)
        {
            case Credential credential:
                SatisfiedByKind = ProviderResourceKind.Credential;
                SatisfiedBySubjectId = credential.SubjectId;
                SatisfiedBySystemId = credential.SystemId;
                SatisfiedByBadgeTypeId = credential.BadgeTypeId;
                SatisfiedByBadgeNumber = credential.BadgeNumber;
                SatisfiedByAccessLevelTypeId = null;
                break;
            case AccessLevel accessLevel:
                SatisfiedByKind = ProviderResourceKind.AccessLevel;
                SatisfiedBySubjectId = accessLevel.SubjectId;
                SatisfiedBySystemId = accessLevel.SystemId;
                SatisfiedByBadgeTypeId = null;
                SatisfiedByBadgeNumber = null;
                SatisfiedByAccessLevelTypeId = accessLevel.AccessLevelTypeId;
                break;
            default:
                ClearSatisfiedBy();
                break;
        }
    }

    public Result<AccessPolicyErrors> MarkReconciliationFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(AccessPolicyErrors.ReconciliationFailureReasonRequired);

        ReconciliationStatus = ReconciliationStatus.ReconciliationFailed;
        ReconciliationFailureReason = reason;
        ClearSatisfiedBy();
        return Result.Success<AccessPolicyErrors>();
    }

    private void ClearSatisfiedBy()
    {
        SatisfiedByKind = null;
        SatisfiedBySubjectId = null;
        SatisfiedBySystemId = null;
        SatisfiedByBadgeTypeId = null;
        SatisfiedByBadgeNumber = null;
        SatisfiedByAccessLevelTypeId = null;
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
