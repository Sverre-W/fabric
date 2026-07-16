using Fabric.Server.Employees.Domain;
using Fabric.Server.Identities.Domain;

namespace Fabric.Server.Employees.Contracts;

public sealed record IdentitySummaryResponse(Guid Id, string DisplayName, string? Email, IdentityStatus Status);

public sealed record OrganizationUnitSummaryResponse(Guid Id, string Name, string? Code, string Type, Guid? ParentId, bool IsActive);

public sealed record EmployeeResponse(
    Guid Id,
    Guid IdentityId,
    IdentitySummaryResponse? Identity,
    string? EmployeeNumber,
    OrganizationUnitSummaryResponse OrganizationUnit,
    Guid? ManagerEmployeeId,
    string? JobTitle,
    EmployeeStatus Status,
    DateOnly? HireDate,
    DateOnly? TerminationDate,
    DateTimeOffset? LeaveStartedAt,
    DateTimeOffset? SuspendedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record OrganizationUnitResponse(
    Guid Id,
    string Name,
    string? Code,
    string Type,
    Guid? ParentId,
    bool IsActive,
    int? Depth,
    int? ChildCount,
    int? EmployeeCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public static class EmployeeMapper
{
    public static OrganizationUnitSummaryResponse ToSummary(this OrganizationUnit unit) =>
        new(unit.Id, unit.Name, unit.Code, unit.Type, unit.ParentId, unit.IsActive);

    public static OrganizationUnitResponse ToResponse(this OrganizationUnit unit, int? depth = null, int? childCount = null, int? employeeCount = null) =>
        new(unit.Id, unit.Name, unit.Code, unit.Type, unit.ParentId, unit.IsActive, depth, childCount, employeeCount, unit.CreatedAt, unit.UpdatedAt);

    public static IdentitySummaryResponse ToEmployeeSummary(this Identity identity) =>
        new(identity.Id, identity.DisplayName, identity.Email, identity.Status);

    public static EmployeeResponse ToResponse(this Employee employee, OrganizationUnit organizationUnit, Identity? identity) =>
        new(
            employee.Id,
            employee.IdentityId,
            identity?.ToEmployeeSummary(),
            employee.EmployeeNumber,
            organizationUnit.ToSummary(),
            employee.ManagerEmployeeId,
            employee.JobTitle,
            employee.Status,
            employee.HireDate,
            employee.TerminationDate,
            employee.LeaveStartedAt,
            employee.SuspendedAt,
            employee.CreatedAt,
            employee.UpdatedAt);
}
