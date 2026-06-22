namespace Fabric.Server.Reception.Domain;

public enum ReceptionErrors
{
    AlreadyOnboarded,
    ArrivalNotFound,
    NotYetOnboarded,
    AlreadyOffboarded,
    InvalidStatus,
    MissingRequiredDocuments,
    NotAVisitor,
    ExpectedArrivalMustBeBeforeExpectedOffboard,
    GracePeriodMustNotBeNegative,
    AccessRuleAssignmentNotFound
}
