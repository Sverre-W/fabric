using Fabric.Server.CredentialManagement.Domain;

namespace Fabric.Server.CredentialManagement.Contracts;

public static class CredentialManagementMappings
{
    public static CredentialTypeResponse ToResponse(
        this CredentialType credentialType,
        int usedCount,
        CredentialTypeTargetResponse[]? targets = null)
    {
        int rangeSize = credentialType.RangeStop - credentialType.RangeStart + 1;
        int availableCount = Math.Max(0, rangeSize - usedCount);
        CredentialCapacityState capacityState = availableCount == 0
            ? CredentialCapacityState.Limit
            : credentialType.NearLimitThreshold.HasValue && availableCount <= credentialType.NearLimitThreshold.Value
                ? CredentialCapacityState.NearLimit
                : CredentialCapacityState.Healthy;

        return new CredentialTypeResponse(
            credentialType.Id,
            credentialType.Name,
            credentialType.Technology,
            credentialType.RangeStart,
            credentialType.RangeStop,
            rangeSize,
            usedCount,
            availableCount,
            credentialType.NearLimitThreshold,
            capacityState,
            credentialType.Status,
            credentialType.CreatedAt,
            credentialType.UpdatedAt,
            targets ?? credentialType.Targets.Select(ToResponse).ToArray());
    }

    public static CredentialTypeTargetResponse ToResponse(this CredentialTypeTarget target) =>
        new(
            target.Id,
            target.CredentialTypeId,
            target.AccessControlSystemId,
            target.ProviderCredentialTypeId,
            target.ProvisioningTiming,
            target.IsEnabled,
            target.CreatedAt,
            target.UpdatedAt);

    public static CredentialReservationResponse ToResponse(this CredentialReservation reservation) =>
        new(
            reservation.Id,
            reservation.CredentialTypeId,
            reservation.CredentialNumber,
            reservation.IdentityId,
            reservation.Status,
            reservation.Purpose,
            reservation.SourceKind,
            reservation.SourceId,
            reservation.RequestedByIdentityId,
            reservation.ReasonText,
            reservation.ExpiresAt,
            reservation.ConsumedAt,
            reservation.ReleasedAt,
            reservation.CreatedAt,
            reservation.UpdatedAt);

    public static CredentialResponse ToResponse(
        this Credential credential,
        CredentialProvisioningTransactionResponse[]? provisioning = null) =>
        new(
            credential.Id,
            credential.CredentialTypeId,
            credential.CredentialNumber,
            credential.IdentityId,
            credential.ReservationId,
            credential.DurationKind,
            credential.ValidFrom,
            credential.ValidUntil,
            credential.Status,
            credential.Purpose,
            credential.SourceKind,
            credential.SourceId,
            credential.RequestedByIdentityId,
            credential.ReasonText,
            credential.CreatedAt,
            credential.UpdatedAt,
            provisioning ?? []);

    public static CredentialProvisioningTransactionResponse ToResponse(this CredentialProvisioningTransaction transaction) =>
        new(
            transaction.Id,
            transaction.CredentialId,
            transaction.CredentialTypeTargetId,
            transaction.AccessControlSystemId,
            transaction.Status,
            transaction.ScheduledFor,
            transaction.AttemptCount,
            transaction.LastAttemptAt,
            transaction.ProvisionedAt,
            transaction.RevokedAt,
            transaction.ErrorMessage,
            transaction.CreatedAt,
            transaction.UpdatedAt);
}
