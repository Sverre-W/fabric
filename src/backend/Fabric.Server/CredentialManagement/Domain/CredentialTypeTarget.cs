namespace Fabric.Server.CredentialManagement.Domain;

public sealed class CredentialTypeTarget
{
    private CredentialTypeTarget() { }

    public Guid Id { get; private set; }
    public Guid CredentialTypeId { get; private set; }
    public Guid AccessControlSystemId { get; private set; }
    public Guid? ProviderCredentialTypeId { get; private set; }
    public ProvisioningTiming ProvisioningTiming { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static CredentialTypeTarget Create(
        Guid credentialTypeId,
        Guid accessControlSystemId,
        Guid? providerCredentialTypeId,
        ProvisioningTiming provisioningTiming,
        DateTimeOffset now) =>
        new()
        {
            Id = Guid.NewGuid(),
            CredentialTypeId = credentialTypeId,
            AccessControlSystemId = accessControlSystemId,
            ProviderCredentialTypeId = providerCredentialTypeId,
            ProvisioningTiming = provisioningTiming,
            IsEnabled = true,
            CreatedAt = now,
            UpdatedAt = now
        };

    public void Update(Guid? providerCredentialTypeId, ProvisioningTiming provisioningTiming, bool isEnabled, DateTimeOffset now)
    {
        ProviderCredentialTypeId = providerCredentialTypeId;
        ProvisioningTiming = provisioningTiming;
        IsEnabled = isEnabled;
        UpdatedAt = now;
    }

    public void SetEnabled(bool enabled, DateTimeOffset now)
    {
        IsEnabled = enabled;
        UpdatedAt = now;
    }
}
