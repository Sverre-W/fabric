using Fabric.Server.Reception.Domain;
using Riok.Mapperly.Abstractions;

namespace Fabric.Server.Reception.Contracts;

public sealed record CreateAccessRuleAssignmentRequest(
    Guid LocationId,
    Guid SystemId,
    Guid AccessLevelTypeId,
    int GracePeriodMinutes,
    ReceptionAccessPolicyTrigger Trigger);

public sealed record UpdateAccessRuleAssignmentRequest(
    Guid LocationId,
    Guid SystemId,
    Guid AccessLevelTypeId,
    int GracePeriodMinutes,
    ReceptionAccessPolicyTrigger Trigger);

public sealed record AccessRuleAssignmentResponse(
    Guid Id,
    Guid LocationId,
    Guid SystemId,
    Guid AccessLevelTypeId,
    int GracePeriodMinutes,
    ReceptionAccessPolicyTrigger Trigger);

[Mapper]
public static partial class AccessRuleAssignmentMapper
{
    public static partial AccessRuleAssignmentResponse ToResponse(this ReceptionAccessRuleAssignment assignment);
}
