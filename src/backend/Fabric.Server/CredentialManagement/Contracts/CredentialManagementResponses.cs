using Fabric.Server.CredentialManagement.Domain;

namespace Fabric.Server.CredentialManagement.Contracts;

public sealed record CredentialTypeResponse(
    Guid Id,
    string Name,
    CredentialTechnology Technology,
    int RangeStart,
    int RangeStop,
    int RangeSize,
    int UsedCount,
    int AvailableCount,
    int? NearLimitThreshold,
    CredentialCapacityState CapacityState,
    CredentialTypeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    CredentialTypeTargetResponse[] Targets);

public sealed record CredentialTypeTargetResponse(
    Guid Id,
    Guid CredentialTypeId,
    Guid AccessControlSystemId,
    Guid? ProviderCredentialTypeId,
    ProvisioningTiming ProvisioningTiming,
    bool IsEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CredentialReservationResponse(
    Guid Id,
    Guid CredentialTypeId,
    int CredentialNumber,
    Guid IdentityId,
    CredentialReservationStatus Status,
    CredentialPurpose Purpose,
    CredentialSourceKind SourceKind,
    Guid? SourceId,
    Guid? RequestedByIdentityId,
    string ReasonText,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? ConsumedAt,
    DateTimeOffset? ReleasedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CredentialResponse(
    Guid Id,
    Guid CredentialTypeId,
    int CredentialNumber,
    Guid IdentityId,
    Guid? ReservationId,
    CredentialDurationKind DurationKind,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil,
    CredentialStatus Status,
    CredentialPurpose Purpose,
    CredentialSourceKind SourceKind,
    Guid? SourceId,
    Guid? RequestedByIdentityId,
    string ReasonText,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    CredentialProvisioningTransactionResponse[] Provisioning);

public sealed record CredentialProvisioningTransactionResponse(
    Guid Id,
    Guid CredentialId,
    Guid CredentialTypeTargetId,
    Guid AccessControlSystemId,
    CredentialProvisioningStatus Status,
    DateTimeOffset ScheduledFor,
    int AttemptCount,
    DateTimeOffset? LastAttemptAt,
    DateTimeOffset? ProvisionedAt,
    DateTimeOffset? RevokedAt,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
