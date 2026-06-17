namespace Fabric.Server.AccessPolicies.Domain;

using Fabric.Server.Core;

public abstract class AccessControlSystem
{
    private protected AccessControlSystem() { }

    public Guid Id { get; protected set; }
    public string Name { get; protected set; } = null!;

    public void Rename(string name) => Name = name;
}

public sealed class UnipassAccessControlSystem : AccessControlSystem
{
    private UnipassAccessControlSystem() { }

    public List<UnipassBadgeType> BadgeTypes { get; private set; } = [];
    public List<UnipassAccessLevelType> AccessLevels { get; private set; } = [];
    public UnipassSystemConfig Config { get; private set; } = null!;

    public static UnipassAccessControlSystem Create(
        Guid id,
        string name,
        List<UnipassBadgeType> badgeTypes,
        List<UnipassAccessLevelType> accessLevels,
        UnipassSystemConfig config) =>
        new()
        {
            Id = id,
            Name = name,
            BadgeTypes = badgeTypes,
            AccessLevels = accessLevels,
            Config = config
        };

    public Result<UnipassBadgeType, AccessControlSystemErrors> AddBadgeType(string name, BadgeRange range)
    {
        if (range.Start > range.Stop)
            return Result.Failure<UnipassBadgeType, AccessControlSystemErrors>(AccessControlSystemErrors.BadgeRangeInvalid);

        if (BadgeTypes.Any(type => type.Name == name))
            return Result.Failure<UnipassBadgeType, AccessControlSystemErrors>(AccessControlSystemErrors.BadgeTypeAlreadyExists);

        var badgeType = UnipassBadgeType.Create(Guid.NewGuid(), Id, name, range);
        BadgeTypes.Add(badgeType);
        return Result.Success<UnipassBadgeType, AccessControlSystemErrors>(badgeType);
    }

    public Result<AccessControlSystemErrors> RemoveBadgeType(Guid badgeTypeId)
    {
        UnipassBadgeType? badgeType = BadgeTypes.SingleOrDefault(type => type.Id == badgeTypeId);
        if (badgeType is null)
            return Result.Failure(AccessControlSystemErrors.BadgeTypeNotFound);

        BadgeTypes.Remove(badgeType);
        return Result.Success<AccessControlSystemErrors>();
    }

    public Result<UnipassAccessLevelType, AccessControlSystemErrors> AddAccessLevel(
        string name,
        int siteId,
        int accessRuleId,
        UnipassMetadata metadata)
    {
        if (!MetadataContainsInt(metadata.Sites, siteId))
            return Result.Failure<UnipassAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.SiteNotFoundInMetadata);

        if (!MetadataContainsInt(metadata.AccessRules, accessRuleId))
            return Result.Failure<UnipassAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.AccessRuleNotFoundInMetadata);

        if (AccessLevels.Any(type => type.Name == name || type.SiteId == siteId && type.AccessRuleId == accessRuleId))
            return Result.Failure<UnipassAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.AccessLevelTypeAlreadyExists);

        var accessLevel = UnipassAccessLevelType.Create(Guid.NewGuid(), Id, name, siteId, accessRuleId);
        AccessLevels.Add(accessLevel);
        return Result.Success<UnipassAccessLevelType, AccessControlSystemErrors>(accessLevel);
    }

    public Result<AccessControlSystemErrors> RemoveAccessLevel(Guid accessLevelTypeId)
    {
        UnipassAccessLevelType? accessLevel = AccessLevels.SingleOrDefault(type => type.Id == accessLevelTypeId);
        if (accessLevel is null)
            return Result.Failure(AccessControlSystemErrors.AccessLevelTypeNotFound);

        AccessLevels.Remove(accessLevel);
        return Result.Success<AccessControlSystemErrors>();
    }

    private static bool MetadataContainsInt(IEnumerable<SystemMetadataObject> metadata, int id) =>
        metadata.Any(item => int.TryParse(item.Id, out int metadataId) && metadataId == id);

    public Result<AccessControlSystemErrors> UpdateConfig(UnipassSystemConfig config)
    {
        if (!config.IsValid())
            return Result.Failure(AccessControlSystemErrors.ConfigInvalid);

        Config = config;
        return Result.Success<AccessControlSystemErrors>();
    }
}

public sealed class LenelAccessControlSystem : AccessControlSystem
{
    private LenelAccessControlSystem() { }

    public List<LenelBadgeType> BadgeTypes { get; private set; } = [];
    public List<LenelAccessLevelType> AccessLevels { get; private set; } = [];
    public LenelSystemConfig Config { get; private set; } = null!;

    public static LenelAccessControlSystem Create(
        Guid id,
        string name,
        List<LenelBadgeType> badgeTypes,
        List<LenelAccessLevelType> accessLevels,
        LenelSystemConfig config) =>
        new()
        {
            Id = id,
            Name = name,
            BadgeTypes = badgeTypes,
            AccessLevels = accessLevels,
            Config = config
        };

    public Result<LenelBadgeType, AccessControlSystemErrors> AddBadgeType(
        string name,
        Guid badgeTypeId,
        LenelMetadata metadata)
    {
        if (!MetadataContainsGuid(metadata.BadgeTypes, badgeTypeId))
            return Result.Failure<LenelBadgeType, AccessControlSystemErrors>(AccessControlSystemErrors.BadgeTypeNotFoundInMetadata);

        if (BadgeTypes.Any(type => type.Name == name || type.BadgeTypeId == badgeTypeId))
            return Result.Failure<LenelBadgeType, AccessControlSystemErrors>(AccessControlSystemErrors.BadgeTypeAlreadyExists);

        var badgeType = LenelBadgeType.Create(Guid.NewGuid(), Id, name, badgeTypeId);
        BadgeTypes.Add(badgeType);
        return Result.Success<LenelBadgeType, AccessControlSystemErrors>(badgeType);
    }

    public Result<AccessControlSystemErrors> RemoveBadgeType(Guid badgeTypeId)
    {
        LenelBadgeType? badgeType = BadgeTypes.SingleOrDefault(type => type.Id == badgeTypeId);
        if (badgeType is null)
            return Result.Failure(AccessControlSystemErrors.BadgeTypeNotFound);

        BadgeTypes.Remove(badgeType);
        return Result.Success<AccessControlSystemErrors>();
    }

    public Result<LenelAccessLevelType, AccessControlSystemErrors> AddAccessLevel(
        string name,
        Guid accessLevelId,
        List<LenelBadgeType> badgeTypes,
        LenelMetadata metadata)
    {
        if (!MetadataContainsGuid(metadata.AccessLevels, accessLevelId))
            return Result.Failure<LenelAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.AccessLevelNotFoundInMetadata);

        if (badgeTypes.Any(badgeType => !MetadataContainsGuid(metadata.BadgeTypes, badgeType.BadgeTypeId)))
            return Result.Failure<LenelAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.BadgeTypeNotFoundInMetadata);

        if (AccessLevels.Any(type => type.Name == name || type.AccessLevelId == accessLevelId))
            return Result.Failure<LenelAccessLevelType, AccessControlSystemErrors>(AccessControlSystemErrors.AccessLevelTypeAlreadyExists);

        var accessLevel = LenelAccessLevelType.Create(Guid.NewGuid(), Id, name, accessLevelId, badgeTypes);
        AccessLevels.Add(accessLevel);
        return Result.Success<LenelAccessLevelType, AccessControlSystemErrors>(accessLevel);
    }

    public Result<AccessControlSystemErrors> RemoveAccessLevel(Guid accessLevelTypeId)
    {
        LenelAccessLevelType? accessLevel = AccessLevels.SingleOrDefault(type => type.Id == accessLevelTypeId);
        if (accessLevel is null)
            return Result.Failure(AccessControlSystemErrors.AccessLevelTypeNotFound);

        AccessLevels.Remove(accessLevel);
        return Result.Success<AccessControlSystemErrors>();
    }

    public Result<AccessControlSystemErrors> UpdateConfig(LenelSystemConfig config)
    {
        if (!config.IsValid())
            return Result.Failure(AccessControlSystemErrors.ConfigInvalid);

        Config = config;
        return Result.Success<AccessControlSystemErrors>();
    }

    private static bool MetadataContainsGuid(IEnumerable<SystemMetadataObject> metadata, Guid id) =>
        metadata.Any(item => Guid.TryParse(item.Id, out Guid metadataId) && metadataId == id);
}

public sealed record UnipassSystemConfig
{
    private UnipassSystemConfig() { }

    private UnipassSystemConfig(string endpoint, bool sslValidation, string username, string password)
    {
        Endpoint = endpoint;
        SslValidation = sslValidation;
        Username = username;
        Password = password;
    }

    public string Endpoint { get; private init; } = null!;
    public bool SslValidation { get; private init; }
    public string Username { get; private init; } = null!;
    public string Password { get; private init; } = null!;

    public static Result<UnipassSystemConfig, AccessControlSystemErrors> Create(
        string endpoint,
        bool sslValidation,
        string username,
        string password)
    {
        var config = new UnipassSystemConfig(endpoint, sslValidation, username, password);
        return config.IsValid()
            ? Result.Success<UnipassSystemConfig, AccessControlSystemErrors>(config)
            : Result.Failure<UnipassSystemConfig, AccessControlSystemErrors>(AccessControlSystemErrors.ConfigInvalid);
    }

    internal bool IsValid() =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);
}

public sealed record LenelSystemConfig
{
    private LenelSystemConfig() { }

    private LenelSystemConfig(string endpoint, bool sslValidation, string apiKey)
    {
        Endpoint = endpoint;
        SslValidation = sslValidation;
        ApiKey = apiKey;
    }

    public string Endpoint { get; private init; } = null!;
    public bool SslValidation { get; private init; }
    public string ApiKey { get; private init; } = null!;

    public static Result<LenelSystemConfig, AccessControlSystemErrors> Create(
        string endpoint,
        bool sslValidation,
        string apiKey)
    {
        var config = new LenelSystemConfig(endpoint, sslValidation, apiKey);
        return config.IsValid()
            ? Result.Success<LenelSystemConfig, AccessControlSystemErrors>(config)
            : Result.Failure<LenelSystemConfig, AccessControlSystemErrors>(AccessControlSystemErrors.ConfigInvalid);
    }

    internal bool IsValid() =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(ApiKey);
}
