namespace Fabric.Server.Hardware.Domain;

public class HardwareAgent
{
    public string Id { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool Enabled { get; private set; }
    public string ApiKeyHash { get; private set; } = default!;
    public string ApiKeySalt { get; private set; } = default!;
    public DateTimeOffset? LastSeenAt { get; private set; }
    public DateTimeOffset? LastInventoryAt { get; private set; }

    public static HardwareAgent Create(string id, string name, string apiKeyHash, string apiKeySalt) => new()
    {
        Id = NormalizeId(id),
        Name = name.Trim(),
        Enabled = true,
        ApiKeyHash = apiKeyHash,
        ApiKeySalt = apiKeySalt
    };

    public void Update(string name, bool enabled)
    {
        Name = name.Trim();
        Enabled = enabled;
    }

    public void RotateKey(string apiKeyHash, string apiKeySalt)
    {
        ApiKeyHash = apiKeyHash;
        ApiKeySalt = apiKeySalt;
    }

    public void MarkSeen(DateTimeOffset seenAt) => LastSeenAt = seenAt;

    public void MarkInventoryReported(DateTimeOffset reportedAt)
    {
        LastSeenAt = reportedAt;
        LastInventoryAt = reportedAt;
    }

    public static string NormalizeId(string id) => id.Trim().ToLowerInvariant();
}
