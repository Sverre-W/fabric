namespace Fabric.Server.Reception.Domain;

public sealed class ReceptionAssignedAccessPolicy
{
    private ReceptionAssignedAccessPolicy() { }

    public Guid Id { get; private set; }
    public Guid ArrivalId { get; private set; }
    public Guid RuleAssignmentId { get; private set; }
    public Guid AccessPolicyId { get; private set; }
    public Guid SystemId { get; private set; }
    public Guid AccessLevelTypeId { get; private set; }

    public static ReceptionAssignedAccessPolicy Create(
        Guid arrivalId,
        Guid ruleAssignmentId,
        Guid accessPolicyId,
        Guid systemId,
        Guid accessLevelTypeId) =>
        new()
        {
            Id = Guid.NewGuid(),
            ArrivalId = arrivalId,
            RuleAssignmentId = ruleAssignmentId,
            AccessPolicyId = accessPolicyId,
            SystemId = systemId,
            AccessLevelTypeId = accessLevelTypeId
        };
}
