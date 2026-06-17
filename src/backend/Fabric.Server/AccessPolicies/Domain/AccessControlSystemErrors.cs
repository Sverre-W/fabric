namespace Fabric.Server.AccessPolicies.Domain;

public enum AccessControlSystemErrors
{
    SystemNotFound,
    SystemProviderMismatch,
    BadgeTypeNotFound,
    AccessLevelTypeNotFound,
    BadgeTypeAlreadyExists,
    AccessLevelTypeAlreadyExists,
    BadgeTypeInUse,
    AccessLevelTypeInUse,
    BadgeRangeInvalid,
    LenelBadgeTypesNotFound,
    ConfigInvalid,
    SiteNotFoundInMetadata,
    AccessRuleNotFoundInMetadata,
    BadgeTypeNotFoundInMetadata,
    AccessLevelNotFoundInMetadata
}
