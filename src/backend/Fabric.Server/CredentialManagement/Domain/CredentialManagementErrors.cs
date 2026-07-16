namespace Fabric.Server.CredentialManagement.Domain;

public enum CredentialManagementErrors
{
    CredentialTypeNotFound,
    CredentialTypeAlreadyExists,
    CredentialTypeDisabled,
    CredentialTypeRangeInvalid,
    CredentialNumberOutsideRange,
    CredentialNumberUnavailable,
    CredentialTypeTargetNotFound,
    CredentialReservationNotFound,
    CredentialReservationNotActive,
    CredentialReservationExpired,
    CredentialReservationIdentityMismatch,
    CredentialNotFound,
    TemporaryCredentialRequiresValidUntil,
    ValidUntilMustBeAfterValidFrom,
    ReasonRequired
}
