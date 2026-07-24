namespace Fabric.Server.CredentialManagement.Domain;

public enum CredentialTechnology
{
    Qr,
    Desfire,
    LicensePlate
}

public enum CredentialTypeStatus
{
    Active,
    Disabled
}

public enum CredentialCapacityState
{
    Healthy,
    NearLimit,
    Limit
}

public enum CredentialDurationKind
{
    Permanent,
    Temporary
}

public enum CredentialAllocationMode
{
    Range,
    Provided
}

public enum CredentialStatus
{
    Issued,
    Active,
    Suspended,
    Expired,
    Revoked,
    Archived
}

public enum CredentialReservationStatus
{
    Active,
    Consumed,
    Released,
    Expired
}

public enum ProvisioningTiming
{
    Immediate,
    AtValidFrom
}

public enum CredentialProvisioningStatus
{
    Pending,
    Provisioned,
    Failed,
    Revoked
}

public enum CredentialPurpose
{
    VisitorAccess,
    EmployeeCredential,
    Replacement,
    TemporaryAccess,
    ManualIssue
}

public enum CredentialSourceKind
{
    VisitInvitation,
    Visit,
    Employee,
    Manual,
    Replacement
}
