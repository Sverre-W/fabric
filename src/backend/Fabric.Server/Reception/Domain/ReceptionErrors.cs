namespace Fabric.Server.Reception.Domain;

public enum ReceptionErrors
{
    AlreadyOnboarded,
    ArrivalNotFound,
    NotYetOnboarded,
    AlreadyOffboarded,
    InvalidStatus,
    MissingRequiredDocuments,
    InvalidIdentityVerificationMethod,
    NotAVisitor,
    ExpectedArrivalMustBeBeforeExpectedOffboard,
    GracePeriodMustNotBeNegative,
    AccessRuleAssignmentNotFound
}
