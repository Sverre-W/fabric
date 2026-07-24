using Fabric.Server.Core;

namespace Fabric.Server.CredentialManagement.Domain;

public sealed class CredentialType
{
    private readonly List<CredentialRange> _ranges = [];

    private CredentialType() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public CredentialTechnology Technology { get; private set; }
    public CredentialAllocationMode AllocationMode { get; private set; }
    public int? NearLimitThreshold { get; private set; }
    public CredentialTypeStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<CredentialRange> Ranges => _ranges;

    public static Result<CredentialType, CredentialManagementErrors> Create(
        string name,
        CredentialTechnology technology,
        CredentialAllocationMode allocationMode,
        int? nearLimitThreshold,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || nearLimitThreshold is < 0)
            return Result.Failure<CredentialType, CredentialManagementErrors>(CredentialManagementErrors.CredentialTypeInvalid);

        return Result.Success<CredentialType, CredentialManagementErrors>(new CredentialType
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Technology = technology,
            AllocationMode = allocationMode,
            NearLimitThreshold = nearLimitThreshold,
            Status = CredentialTypeStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    public Result<CredentialManagementErrors> Update(
        string name,
        CredentialTechnology technology,
        CredentialAllocationMode allocationMode,
        int? nearLimitThreshold,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || nearLimitThreshold is < 0)
            return Result.Failure(CredentialManagementErrors.CredentialTypeInvalid);

        Name = name.Trim();
        Technology = technology;
        AllocationMode = allocationMode;
        NearLimitThreshold = nearLimitThreshold;
        UpdatedAt = now;
        return Result.Success<CredentialManagementErrors>();
    }

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
