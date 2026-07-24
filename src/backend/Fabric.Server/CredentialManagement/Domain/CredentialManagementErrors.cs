namespace Fabric.Server.CredentialManagement.Domain;

public enum CredentialManagementErrors
{
    CredentialTypeNotFound,
    CredentialTypeAlreadyExists,
    CredentialTypeDisabled,
    CredentialTypeInvalid,
    CredentialRangeInvalid,
    CredentialRangeNotFound,
    CredentialIdentifierRequired,
    CredentialIdentifierAlreadyExists,
    CredentialIdentifierOutsideRange,
    CredentialIdentifierUnavailable,
    CredentialIdentifierMustBeNumeric,
    CredentialNotFound,
    TemporaryCredentialRequiresValidUntil,
    PermanentCredentialMustNotHaveValidUntil,
    ValidUntilMustBeAfterValidFrom,
    ReasonRequired
}
