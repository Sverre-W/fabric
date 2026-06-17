namespace Fabric.Server.Visitors.Domain;

public sealed class Organizer()
{

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public bool Active { get; private set; }

    public static Organizer Create(string firstName, string lastName, string email)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Active = true
        };
    }

    public void Deactivate() => Active = false;

    public void Activate() => Active = true;
}

public sealed class Visitor
{
    internal Visitor() { }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string? Company { get; private set; }

    public static Visitor Create(Guid id, string firstName, string lastName, string email, string company)
    {
        return new Visitor
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Company = company
        };
    }

    public void UpdateProfile(string firstName, string lastName, string email, string company)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Company = company;
    }
}
