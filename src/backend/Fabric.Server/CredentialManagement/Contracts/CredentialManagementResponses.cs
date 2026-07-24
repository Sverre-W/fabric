using Fabric.Server.CredentialManagement.Domain;

namespace Fabric.Server.CredentialManagement.Contracts;

public sealed record CredentialTypeResponse(
    Guid Id,
    string Name,
    CredentialTechnology Technology,
    CredentialAllocationMode AllocationMode,
    int UsedCount,
    int AvailableCount,
    int? NearLimitThreshold,
    CredentialCapacityState CapacityState,
    CredentialTypeStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    CredentialRangeResponse[] Ranges);

public sealed record CredentialRangeResponse(
    Guid Id,
    Guid CredentialTypeId,
    long RangeStart,
    long RangeStop,
    bool IsActive);

public sealed record CredentialResponse(
    Guid Id,
    Guid CredentialTypeId,
    string Identifier,
    Guid IdentityId,
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
    DateTimeOffset UpdatedAt);
