namespace Fabric.Server.AccessPolicies.Domain;

public sealed class Subject
{
    private Subject() { }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public SubjectType SubjectType { get; private set; }

    public static Subject Create(Guid id, string firstName, string lastName, SubjectType subjectType) =>
        new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            SubjectType = subjectType
        };
}

public enum SubjectType
{
    Employee,
    Contractor,
    Visitor
}
