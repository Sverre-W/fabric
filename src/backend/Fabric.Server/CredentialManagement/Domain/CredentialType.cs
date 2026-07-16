using Fabric.Server.Core;

namespace Fabric.Server.CredentialManagement.Domain;

public sealed class CredentialType
{
    private readonly List<CredentialTypeTarget> _targets = [];

    private CredentialType() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public CredentialTechnology Technology { get; private set; }
    public int RangeStart { get; private set; }
    public int RangeStop { get; private set; }
    public int? NearLimitThreshold { get; private set; }
    public CredentialTypeStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<CredentialTypeTarget> Targets => _targets;

    public static Result<CredentialType, CredentialManagementErrors> Create(
        string name,
        CredentialTechnology technology,
        int rangeStart,
        int rangeStop,
        int? nearLimitThreshold,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || rangeStart > rangeStop || nearLimitThreshold is < 0)
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeRangeInvalid);

        return Result.Success<CredentialType, CredentialManagementErrors>(new CredentialType
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Technology = technology,
            RangeStart = rangeStart,
            RangeStop = rangeStop,
            NearLimitThreshold = nearLimitThreshold,
            Status = CredentialTypeStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    public Result<CredentialManagementErrors> Update(
        string name,
        CredentialTechnology technology,
        int rangeStart,
        int rangeStop,
        int? nearLimitThreshold,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || rangeStart > rangeStop || nearLimitThreshold is < 0)
            return Result.Failure(CredentialManagementErrors.CredentialTypeRangeInvalid);

        Name = name.Trim();
        Technology = technology;
        RangeStart = rangeStart;
        RangeStop = rangeStop;
        NearLimitThreshold = nearLimitThreshold;
        UpdatedAt = now;
        return Result.Success<CredentialManagementErrors>();
    }

    public bool ContainsNumber(int credentialNumber) =>
        credentialNumber >= RangeStart && credentialNumber <= RangeStop;

    public void Activate(DateTimeOffset now)
    {
        Status = CredentialTypeStatus.Active;
        UpdatedAt = now;
    }

    public void Disable(DateTimeOffset now)
    {
        Status = CredentialTypeStatus.Disabled;
        UpdatedAt = now;
    }
}
