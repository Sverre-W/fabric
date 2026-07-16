using Fabric.Server.Core;
using Fabric.Server.Employees.Domain;

namespace Fabric.Server.Employees.Contracts;

public sealed record ListEmployeesRequest : BaseListRequest
{
    public string? Query { get; set; }
    public EmployeeStatus[]? Status { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public bool IncludeDescendants { get; set; } = false;
}

public sealed record CreateEmployeeRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    string? PreferredName,
    string? Email,
    string? Phone,
    string? EmployeeNumber,
    Guid OrganizationUnitId,
    Guid? ManagerEmployeeId,
    string? JobTitle,
    DateOnly? HireDate,
    EmployeeStatus? Status);

public sealed record UpdateEmployeeWorkDetailsRequest(
    string? EmployeeNumber,
    Guid OrganizationUnitId,
    Guid? ManagerEmployeeId,
    string? JobTitle,
    DateOnly? HireDate);

public sealed record TransitionEmployeeStatusRequest(EmployeeStatus Status, DateOnly? EffectiveDate);
