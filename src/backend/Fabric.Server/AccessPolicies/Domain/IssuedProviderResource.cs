namespace Fabric.Server.AccessPolicies.Domain;

public sealed class IssuedProviderResource
{
    private IssuedProviderResource() { }

    public Guid Id { get; private set; }
    public Guid PolicyId { get; private set; }
    public Guid SubjectId { get; private set; }
    public Guid SystemId { get; private set; }
    public ProviderResourceKind ResourceKind { get; private set; }
    public Guid? BadgeTypeId { get; private set; }
    public int? BadgeNumber { get; private set; }
    public Guid? AccessLevelTypeId { get; private set; }
    public string ExternalPersonId { get; private set; } = null!;
    public string ExternalResourceId { get; private set; } = null!;

    public static IssuedProviderResource CreateCredential(
        Guid policyId,
        Guid subjectId,
        Guid systemId,
        Guid badgeTypeId,
        int badgeNumber,
        string externalPersonId,
        string externalResourceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            SubjectId = subjectId,
            SystemId = systemId,
            ResourceKind = ProviderResourceKind.Credential,
            BadgeTypeId = badgeTypeId,
            BadgeNumber = badgeNumber,
            ExternalPersonId = externalPersonId,
            ExternalResourceId = externalResourceId
        };

    public static IssuedProviderResource CreateAccessLevel(
        Guid policyId,
        Guid subjectId,
        Guid systemId,
        Guid accessLevelTypeId,
        string externalPersonId,
        string externalResourceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            SubjectId = subjectId,
            SystemId = systemId,
            ResourceKind = ProviderResourceKind.AccessLevel,
            AccessLevelTypeId = accessLevelTypeId,
            ExternalPersonId = externalPersonId,
            ExternalResourceId = externalResourceId
        };
}

public enum ProviderResourceKind
{
    Credential,
    AccessLevel
}
