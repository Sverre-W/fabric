using Fabric.Server.Core;

namespace Fabric.Server.Visitors.Contracts;

public record ListOrganizerRequest : BaseListRequest
{
    public string? Query { get; set; }
}

public record AddOrganizerRequest(string FirstName, string LastName, string Email);