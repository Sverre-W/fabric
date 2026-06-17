namespace Fabric.Server.AccessPolicies.Domain;

public abstract class BadgeType
{
    protected BadgeType() { }

    public Guid Id { get; protected set; }
    public Guid SystemId { get; protected set; }
    public string Name { get; protected set; } = null!;
}

public sealed class UnipassBadgeType : BadgeType
{
    private UnipassBadgeType() { }

    public BadgeRange Range { get; private set; } = null!;

    public static UnipassBadgeType Create(Guid id, Guid systemId, string name, BadgeRange range) =>
        new()
        {
            Id = id,
            SystemId = systemId,
            Name = name,
            Range = range
        };
}

public sealed class LenelBadgeType : BadgeType
{
    private LenelBadgeType() { }

    public Guid BadgeTypeId { get; private set; }

    public static LenelBadgeType Create(Guid id, Guid systemId, string name, Guid badgeTypeId) =>
        new()
        {
            Id = id,
            SystemId = systemId,
            Name = name,
            BadgeTypeId = badgeTypeId
        };
}

public abstract class AccessLevelType
{
    protected AccessLevelType() { }

    public Guid Id { get; protected set; }
    public Guid SystemId { get; protected set; }
    public string Name { get; protected set; } = null!;
}

public sealed class UnipassAccessLevelType : AccessLevelType
{
    private UnipassAccessLevelType() { }

    public int SiteId { get; private set; }
    public int AccessRuleId { get; private set; }

    public static UnipassAccessLevelType Create(Guid id, Guid systemId, string name, int siteId, int accessRuleId) =>
        new()
        {
            Id = id,
            SystemId = systemId,
            Name = name,
            SiteId = siteId,
            AccessRuleId = accessRuleId
        };
}

public sealed class LenelAccessLevelType : AccessLevelType
{
    private LenelAccessLevelType() { }

    public Guid AccessLevelId { get; private set; }
    public List<LenelBadgeType> BadgeTypes { get; private set; } = [];

    public static LenelAccessLevelType Create(
        Guid id,
        Guid systemId,
        string name,
        Guid accessLevelId,
        List<LenelBadgeType> badgeTypes) =>
        new()
        {
            Id = id,
            SystemId = systemId,
            Name = name,
            AccessLevelId = accessLevelId,
            BadgeTypes = badgeTypes
        };
}

public sealed record BadgeRange(int Start, int Stop);
