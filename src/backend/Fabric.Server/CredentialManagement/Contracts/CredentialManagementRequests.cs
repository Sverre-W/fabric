using Fabric.Server.Core;
using Fabric.Server.CredentialManagement.Domain;

namespace Fabric.Server.CredentialManagement.Contracts;

public sealed class ListCredentialTypesRequest : Pageable
{
    public string? Query { get; set; }
    public CredentialTechnology? Technology { get; set; }
    public CredentialTypeStatus? Status { get; set; }
}

public sealed class ListCredentialsRequest : Pageable
{
    public Guid? CredentialTypeId { get; set; }
    public Guid? IdentityId { get; set; }
    public CredentialStatus? Status { get; set; }
}

public sealed class ListCredentialReservationsRequest : Pageable
{
    public Guid? CredentialTypeId { get; set; }
    public Guid? IdentityId { get; set; }
    public CredentialReservationStatus? Status { get; set; }
}

public sealed record CreateCredentialTypeRequest(
    string Name,
    CredentialTechnology Technology,
    int RangeStart,
    int RangeStop,
    int? NearLimitThreshold);

public sealed record UpdateCredentialTypeRequest(
    string Name,
    CredentialTechnology Technology,
    int RangeStart,
    int RangeStop,
    int? NearLimitThreshold);

public sealed record CreateCredentialTypeTargetRequest(
    Guid AccessControlSystemId,
    Guid? ProviderCredentialTypeId,
    ProvisioningTiming ProvisioningTiming);

public sealed record UpdateCredentialTypeTargetRequest(
    Guid? ProviderCredentialTypeId,
    ProvisioningTiming ProvisioningTiming,
    bool IsEnabled);

public sealed record CreateCredentialReservationRequest(
    Guid CredentialTypeId,
    int? CredentialNumber,
    Guid IdentityId,
    CredentialPurpose Purpose,
    CredentialSourceKind SourceKind,
    Guid? SourceId,
    Guid? RequestedByIdentityId,
    string ReasonText,
    DateTimeOffset ExpiresAt);

public sealed record IssueCredentialRequest(
    Guid CredentialTypeId,
    int? CredentialNumber,
    Guid? ReservationId,
    Guid IdentityId,
    CredentialDurationKind DurationKind,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidUntil,
    CredentialPurpose Purpose,
    CredentialSourceKind SourceKind,
    Guid? SourceId,
    Guid? RequestedByIdentityId,
    string ReasonText);
