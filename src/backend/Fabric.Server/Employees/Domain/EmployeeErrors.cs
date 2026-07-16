namespace Fabric.Server.Employees.Domain;

public enum EmployeeErrors
{
    EmployeeNotFound,
    OrganizationUnitNotFound,
    OrganizationUnitInactive,
    OrganizationUnitAlreadyExists,
    OrganizationUnitHasActiveChildren,
    OrganizationUnitHasActiveEmployees,
    OrganizationUnitParentCycle,
    EmployeeNumberAlreadyExists,
    ManagerNotFound,
    ManagerCannotBeSelf,
    InvalidEmployeeStatusTransition,
    HireDateRequiredForActiveEmployee,
    TerminationDateRequired,
    EmployeeAlreadyArchived,
    IdentityCreationFailed,
}
