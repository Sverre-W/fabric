namespace Fabric.Server.AccessPolicies.Domain;

public abstract class PolicyRequirement
{
    private protected PolicyRequirement() { }

    public abstract bool BelongsToSystem(Guid systemId);
}

public sealed class CredentialRequirement : PolicyRequirement
{
    private CredentialRequirement() { }

    private CredentialRequirement(BadgeType badgeType, int? badgeNumber)
    {
        BadgeType = badgeType;
        BadgeNumber = badgeNumber;
    }

    public BadgeType BadgeType { get; private set; } = null!;
    public int? BadgeNumber { get; private set; }

    public static CredentialRequirement Create(BadgeType badgeType, int? badgeNumber) =>
        new(badgeType, badgeNumber);

    public override bool BelongsToSystem(Guid systemId) => BadgeType.SystemId == systemId;
}

public sealed class AccessRequirement : PolicyRequirement
{
    private AccessRequirement() { }

    private AccessRequirement(AccessLevelType accessLevel)
    {
        AccessLevel = accessLevel;
    }

    public AccessLevelType AccessLevel { get; private set; } = null!;

    public static AccessRequirement Create(AccessLevelType accessLevel) => new(accessLevel);

    public override bool BelongsToSystem(Guid systemId) => AccessLevel.SystemId == systemId;
}
