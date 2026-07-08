namespace Fabric.Server.Kiosk.Domain;

public sealed class Kiosk
{
    private Kiosk() { }

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public string Name { get; private set; } = default!;
    public KioskMode Mode { get; private set; }
    public string ApiKeyHash { get; private set; } = default!;
    public string ApiKeySalt { get; private set; } = default!;
    public string? WorkflowDefinitionId { get; private set; }
    public DateTimeOffset? LastSeenAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Kiosk Create(string name, Guid profileId, string apiKeyHash, string apiKeySalt, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        ProfileId = profileId,
        Mode = KioskMode.Disabled,
        ApiKeyHash = apiKeyHash,
        ApiKeySalt = apiKeySalt,
        CreatedAt = now,
        UpdatedAt = now
    };

    public void Update(string name, Guid profileId, DateTimeOffset now)
    {
        Name = name.Trim();
        ProfileId = profileId;
        UpdatedAt = now;
    }

    public void AssignWorkflow(string workflowDefinitionId, DateTimeOffset now)
    {
        WorkflowDefinitionId = workflowDefinitionId.Trim();
        UpdatedAt = now;
    }

    public void Activate(DateTimeOffset now) => SetMode(KioskMode.Active, now);

    public void SetMaintenance(DateTimeOffset now) => SetMode(KioskMode.Maintenance, now);

    public void Disable(DateTimeOffset now) => SetMode(KioskMode.Disabled, now);

    public void RotateKey(string apiKeyHash, string apiKeySalt, DateTimeOffset now)
    {
        ApiKeyHash = apiKeyHash;
        ApiKeySalt = apiKeySalt;
        UpdatedAt = now;
    }

    public void MarkSeen(DateTimeOffset seenAt) => LastSeenAt = seenAt;

    private void SetMode(KioskMode mode, DateTimeOffset now)
    {
        Mode = mode;
        UpdatedAt = now;
    }
}
