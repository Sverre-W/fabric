namespace Fabric.Server.AccessPolicies.Domain;

public sealed class IdentityMapping
{
    private IdentityMapping() { }

    public Guid SubjectId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public SubjectType SubjectType { get; private set; }
    public Guid SystemId { get; private set; }
    public string ExternalId { get; private set; } = null!;

    public static IdentityMapping Create(Subject subject, Guid systemId, string externalId) =>
        new()
        {
            SubjectId = subject.Id,
            FirstName = subject.FirstName,
            LastName = subject.LastName,
            SubjectType = subject.SubjectType,
            SystemId = systemId,
            ExternalId = externalId
        };
}
