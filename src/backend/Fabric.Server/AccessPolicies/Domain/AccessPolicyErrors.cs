namespace Fabric.Server.AccessPolicies.Domain;

public enum AccessPolicyErrors
{
    EffectiveFromMustBeBeforeEffectiveUntil,
    ReconciliationFailureReasonRequired,
    RequirementDoesNotBelongToSystem,
    SystemNotFound,
    PolicyNotFound,
    BadgeTypeNotFound,
    AccessLevelTypeNotFound,
    ReconciliationFailed
}
