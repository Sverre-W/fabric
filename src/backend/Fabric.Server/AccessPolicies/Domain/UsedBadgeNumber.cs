namespace Fabric.Server.AccessPolicies.Domain;

public sealed class UsedBadgeNumber
{
    private UsedBadgeNumber() { }

    public Guid Id { get; private set; }
    public Guid SystemId { get; private set; }
    public Guid BadgeTypeId { get; private set; }
    public Guid SubjectId { get; private set; }
    public int BadgeNumber { get; private set; }

    public static UsedBadgeNumber Create(Guid systemId, Guid badgeTypeId, Guid subjectId, int badgeNumber) =>
        new()
        {
            Id = Guid.NewGuid(),
            SystemId = systemId,
            BadgeTypeId = badgeTypeId,
            SubjectId = subjectId,
            BadgeNumber = badgeNumber
        };
}
