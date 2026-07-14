namespace Fabric.Server.Reception.Domain;

public sealed class ReceptionKiosk
{
    private ReceptionKiosk() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Guid LocationId { get; private set; }
    public string ApiKeyHash { get; private set; } = null!;
    public string ApiKeySalt { get; private set; } = null!;
    public bool Enabled { get; private set; }
    public bool RequireFacePicture { get; private set; }
    public IdentityVerificationMethod? IdentityVerificationMethod { get; private set; }

    public static ReceptionKiosk Create(
        string name,
        Guid locationId,
        string apiKeyHash,
        string apiKeySalt,
        bool requireFacePicture,
        IdentityVerificationMethod? identityVerificationMethod) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            LocationId = locationId,
            ApiKeyHash = apiKeyHash,
            ApiKeySalt = apiKeySalt,
            Enabled = true,
            RequireFacePicture = requireFacePicture,
            IdentityVerificationMethod = identityVerificationMethod
        };

    public void Update(
        string name,
        Guid locationId,
        bool enabled,
        bool requireFacePicture,
        IdentityVerificationMethod? identityVerificationMethod)
    {
        Name = name;
        LocationId = locationId;
        Enabled = enabled;
        RequireFacePicture = requireFacePicture;
        IdentityVerificationMethod = identityVerificationMethod;
    }

    public void RotateKey(string apiKeyHash, string apiKeySalt)
    {
        ApiKeyHash = apiKeyHash;
        ApiKeySalt = apiKeySalt;
    }

    public void Disable() => Enabled = false;
}
