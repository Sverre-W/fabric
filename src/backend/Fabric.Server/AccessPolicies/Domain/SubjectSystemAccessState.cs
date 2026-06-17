namespace Fabric.Server.AccessPolicies.Domain;

public sealed record SubjectSystemAccessState(
    Guid SubjectId,
    Guid SystemId,
    IReadOnlyList<IssuedResource> IssuedResources)
{
    public static SubjectSystemAccessState Empty(Guid subjectId, Guid systemId) =>
        new(subjectId, systemId, []);

    public IReadOnlyList<Credential> Credentials => IssuedResources.OfType<Credential>().ToList();

    public IReadOnlyList<AccessLevel> AccessLevels => IssuedResources.OfType<AccessLevel>().ToList();

    public bool Satisfies(AccessPolicy policy, out IssuedResource? resource)
    {
        if (policy.Subject.Id != SubjectId || policy.SystemId != SystemId)
        {
            resource = null;
            return false;
        }

        return Satisfies(policy.Requirement, out resource);
    }

    public bool Satisfies(PolicyRequirement requirement, out IssuedResource? resource)
    {
        resource = IssuedResources.FirstOrDefault(item => item.SubjectId == SubjectId && item.Satisfies(requirement));
        return resource is not null;
    }

    public IReadOnlyList<PolicyRequirement> MissingRequirements(IEnumerable<AccessPolicy> policies) =>
        policies
            .Where(policy => policy.Subject.Id == SubjectId && policy.SystemId == SystemId)
            .Select(policy => policy.Requirement)
            .Where(requirement => !Satisfies(requirement, out _))
            .ToList();
}
