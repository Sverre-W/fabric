namespace Fabric.Server.AccessPolicies.Domain;

public abstract class IssuedResource
{
    protected IssuedResource() { }

    public Guid SubjectId { get; protected set; }
    public Guid SystemId { get; protected set; }

    public abstract bool Satisfies(PolicyRequirement requirement);
}

public abstract class Credential : IssuedResource
{
    protected Credential() { }

    public Guid BadgeTypeId { get; protected set; }
    public string BadgeNumber { get; protected set; } = null!;

    protected bool MatchesBadgeNumber(int? badgeNumber) =>
        !badgeNumber.HasValue || BadgeNumber == badgeNumber.Value.ToString();
}

public sealed class UnipassCredential : Credential
{
    private UnipassCredential() { }

    public static UnipassCredential Create(
        Guid subjectId,
        Guid badgeTypeId,
        Guid systemId,
        string badgeNumber) =>
        new()
        {
            SubjectId = subjectId,
            BadgeTypeId = badgeTypeId,
            SystemId = systemId,
            BadgeNumber = badgeNumber
        };

    public override bool Satisfies(PolicyRequirement requirement) =>
        requirement is CredentialRequirement credential &&
        SystemId == credential.BadgeType.SystemId &&
        BadgeTypeId == credential.BadgeType.Id &&
        MatchesBadgeNumber(credential.BadgeNumber);
}

public sealed class LenelCredential : Credential
{
    private LenelCredential() { }

    public static LenelCredential Create(
        Guid subjectId,
        Guid badgeTypeId,
        Guid systemId,
        string badgeNumber) =>
        new()
        {
            SubjectId = subjectId,
            BadgeTypeId = badgeTypeId,
            SystemId = systemId,
            BadgeNumber = badgeNumber
        };

    public override bool Satisfies(PolicyRequirement requirement) =>
        requirement is CredentialRequirement credential &&
        SystemId == credential.BadgeType.SystemId &&
        BadgeTypeId == credential.BadgeType.Id &&
        MatchesBadgeNumber(credential.BadgeNumber);
}

public abstract class AccessLevel : IssuedResource
{
    protected AccessLevel() { }

    public Guid AccessLevelTypeId { get; protected set; }
}

public sealed class UnipassAccessLevel : AccessLevel
{
    private UnipassAccessLevel() { }

    public static UnipassAccessLevel Create(Guid subjectId, Guid accessLevelTypeId, Guid systemId) =>
        new()
        {
            SubjectId = subjectId,
            AccessLevelTypeId = accessLevelTypeId,
            SystemId = systemId
        };

    public override bool Satisfies(PolicyRequirement requirement) =>
        requirement is AccessRequirement access &&
        SystemId == access.AccessLevel.SystemId &&
        AccessLevelTypeId == access.AccessLevel.Id;
}

public sealed class LenelAccessLevel : AccessLevel
{
    private LenelAccessLevel() { }

    public static LenelAccessLevel Create(Guid subjectId, Guid accessLevelTypeId, Guid systemId) =>
        new()
        {
            SubjectId = subjectId,
            AccessLevelTypeId = accessLevelTypeId,
            SystemId = systemId
        };

    public override bool Satisfies(PolicyRequirement requirement) =>
        requirement is AccessRequirement access &&
        SystemId == access.AccessLevel.SystemId &&
        AccessLevelTypeId == access.AccessLevel.Id;
}
