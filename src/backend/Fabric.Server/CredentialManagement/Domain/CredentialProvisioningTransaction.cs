namespace Fabric.Server.CredentialManagement.Domain;

public sealed class CredentialProvisioningTransaction
{
    private CredentialProvisioningTransaction() { }

    public Guid Id { get; private set; }
    public Guid CredentialId { get; private set; }
    public Guid CredentialTypeTargetId { get; private set; }
    public Guid AccessControlSystemId { get; private set; }
    public CredentialProvisioningStatus Status { get; private set; }
    public DateTimeOffset ScheduledFor { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public DateTimeOffset? ProvisionedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static CredentialProvisioningTransaction Create(
        Credential credential,
        CredentialTypeTarget target,
        DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            CredentialId = credential.Id,
            CredentialTypeTargetId = target.Id,
            AccessControlSystemId = target.AccessControlSystemId,
            Status = CredentialProvisioningStatus.Pending,
            ScheduledFor = target.ProvisioningTiming == ProvisioningTiming.Immediate ? now : credential.ValidFrom,
            CreatedAt = now,
            UpdatedAt = now
        };
}
