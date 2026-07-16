using Fabric.Server.Core;

namespace Fabric.Server.CredentialManagement.Domain;

public sealed class CredentialReservation
{
    private CredentialReservation() { }

    public Guid Id { get; private set; }
    public Guid CredentialTypeId { get; private set; }
    public int CredentialNumber { get; private set; }
    public Guid IdentityId { get; private set; }
    public CredentialReservationStatus Status { get; private set; }
    public CredentialPurpose Purpose { get; private set; }
    public CredentialSourceKind SourceKind { get; private set; }
    public Guid? SourceId { get; private set; }
    public Guid? RequestedByIdentityId { get; private set; }
    public string ReasonText { get; private set; } = null!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ConsumedAt { get; private set; }
    public DateTimeOffset? ReleasedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<CredentialReservation, CredentialManagementErrors> Create(
        Guid credentialTypeId,
        int credentialNumber,
        Guid identityId,
        CredentialPurpose purpose,
        CredentialSourceKind sourceKind,
        Guid? sourceId,
        Guid? requestedByIdentityId,
        string reasonText,
        DateTimeOffset expiresAt,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(reasonText))
            return Result.Failure<CredentialReservation, CredentialManagementErrors>(CredentialManagementErrors.ReasonRequired);

        return Result.Success<CredentialReservation, CredentialManagementErrors>(new CredentialReservation
        {
            Id = Guid.NewGuid(),
            CredentialTypeId = credentialTypeId,
            CredentialNumber = credentialNumber,
            IdentityId = identityId,
            Status = CredentialReservationStatus.Active,
            Purpose = purpose,
            SourceKind = sourceKind,
            SourceId = sourceId,
            RequestedByIdentityId = requestedByIdentityId,
            ReasonText = reasonText.Trim(),
            ExpiresAt = expiresAt,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    public Result<CredentialManagementErrors> Consume(DateTimeOffset now)
    {
        if (Status != CredentialReservationStatus.Active)
            return Result.Failure(CredentialManagementErrors.CredentialReservationNotActive);

        if (ExpiresAt <= now)
        {
            Status = CredentialReservationStatus.Expired;
            UpdatedAt = now;
            return Result.Failure(CredentialManagementErrors.CredentialReservationExpired);
        }

        Status = CredentialReservationStatus.Consumed;
        ConsumedAt = now;
        UpdatedAt = now;
        return Result.Success<CredentialManagementErrors>();
    }

    public void Release(DateTimeOffset now)
    {
        if (Status != CredentialReservationStatus.Active)
            return;

        Status = CredentialReservationStatus.Released;
        ReleasedAt = now;
        UpdatedAt = now;
    }
}
