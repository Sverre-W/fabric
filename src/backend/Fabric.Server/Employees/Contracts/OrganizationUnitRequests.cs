using Fabric.Server.Core;

namespace Fabric.Server.Employees.Contracts;

public sealed record ListOrganizationUnitsRequest : BaseListRequest
{
    public string? Query { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsActive { get; set; }
}

public sealed record CreateOrganizationUnitRequest(string Name, string? Code, string Type, Guid? ParentId);

public sealed record UpdateOrganizationUnitRequest(string Name, string? Code, string Type);

public sealed record MoveOrganizationUnitRequest(Guid? ParentId);
