using Fabric.Server.Core;

namespace Fabric.Server.Employees.Domain;

public sealed class Employee
{
    private Employee() { }

    public Guid Id { get; private set; }
    public Guid IdentityId { get; private set; }
    public Guid OrganizationUnitId { get; private set; }
    public Guid? ManagerEmployeeId { get; private set; }
    public string? EmployeeNumber { get; private set; }
    public string? JobTitle { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateOnly? HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public DateTimeOffset? LeaveStartedAt { get; private set; }
    public DateTimeOffset? SuspendedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<Employee, EmployeeErrors> Create(
        Guid identityId,
        Guid organizationUnitId,
        Guid? managerEmployeeId,
        string? employeeNumber,
        string? jobTitle,
        EmployeeStatus status,
        DateOnly? hireDate,
        DateTimeOffset now)
    {
        if (status == EmployeeStatus.Active && !hireDate.HasValue)
            return Result.Failure<Employee, EmployeeErrors>(EmployeeErrors.HireDateRequiredForActiveEmployee);

        if (status is EmployeeStatus.Terminated or EmployeeStatus.Archived)
            return Result.Failure<Employee, EmployeeErrors>(EmployeeErrors.InvalidEmployeeStatusTransition);

        DateTimeOffset? leaveStartedAt = status == EmployeeStatus.Leave ? now : null;
        DateTimeOffset? suspendedAt = status == EmployeeStatus.Suspended ? now : null;

        return Result.Success<Employee, EmployeeErrors>(new Employee
        {
            Id = Guid.NewGuid(),
            IdentityId = identityId,
            OrganizationUnitId = organizationUnitId,
            ManagerEmployeeId = managerEmployeeId,
            EmployeeNumber = NormalizeOptional(employeeNumber),
            JobTitle = NormalizeOptional(jobTitle),
            Status = status,
            HireDate = hireDate,
            LeaveStartedAt = leaveStartedAt,
            SuspendedAt = suspendedAt,
            CreatedAt = now,
            UpdatedAt = now,
        });
    }

    public Result<EmployeeErrors> UpdateWorkDetails(
        Guid organizationUnitId,
        Guid? managerEmployeeId,
        string? employeeNumber,
        string? jobTitle,
        DateOnly? hireDate,
        DateTimeOffset now)
    {
        if (managerEmployeeId == Id)
            return Result.Failure(EmployeeErrors.ManagerCannotBeSelf);

        if (Status == EmployeeStatus.Active && !hireDate.HasValue)
            return Result.Failure(EmployeeErrors.HireDateRequiredForActiveEmployee);

        OrganizationUnitId = organizationUnitId;
        ManagerEmployeeId = managerEmployeeId;
        EmployeeNumber = NormalizeOptional(employeeNumber);
        JobTitle = NormalizeOptional(jobTitle);
        HireDate = hireDate;
        UpdatedAt = now;
        return Result.Success<EmployeeErrors>();
    }

    public Result<EmployeeErrors> TransitionTo(EmployeeStatus status, DateOnly? effectiveDate, DateTimeOffset now)
    {
        if (Status == EmployeeStatus.Archived)
            return Result.Failure(EmployeeErrors.EmployeeAlreadyArchived);

        if (status == Status)
            return Result.Success<EmployeeErrors>();

        if (!CanTransition(Status, status))
            return Result.Failure(EmployeeErrors.InvalidEmployeeStatusTransition);

        if (status == EmployeeStatus.Active && !HireDate.HasValue && !effectiveDate.HasValue)
            return Result.Failure(EmployeeErrors.HireDateRequiredForActiveEmployee);

        if (status == EmployeeStatus.Terminated && !effectiveDate.HasValue)
            return Result.Failure(EmployeeErrors.TerminationDateRequired);

        Status = status;
        UpdatedAt = now;

        switch (status)
        {
            case EmployeeStatus.Active:
                HireDate ??= effectiveDate;
                LeaveStartedAt = null;
                SuspendedAt = null;
                break;
            case EmployeeStatus.Leave:
                LeaveStartedAt = now;
                SuspendedAt = null;
                break;
            case EmployeeStatus.Suspended:
                SuspendedAt = now;
                LeaveStartedAt = null;
                break;
            case EmployeeStatus.Terminated:
                TerminationDate = effectiveDate;
                LeaveStartedAt = null;
                SuspendedAt = null;
                break;
            case EmployeeStatus.Archived:
                break;
        }

        return Result.Success<EmployeeErrors>();
    }

    public bool BlocksOrganizationUnitDeactivation() =>
        Status is not EmployeeStatus.Terminated and not EmployeeStatus.Archived;

    private static bool CanTransition(EmployeeStatus from, EmployeeStatus to) =>
        (from, to) switch
        {
            (EmployeeStatus.PreHire, EmployeeStatus.Active) => true,
            (EmployeeStatus.PreHire, EmployeeStatus.Terminated) => true,
            (EmployeeStatus.Active, EmployeeStatus.Leave) => true,
            (EmployeeStatus.Active, EmployeeStatus.Suspended) => true,
            (EmployeeStatus.Active, EmployeeStatus.Terminated) => true,
            (EmployeeStatus.Leave, EmployeeStatus.Active) => true,
            (EmployeeStatus.Leave, EmployeeStatus.Terminated) => true,
            (EmployeeStatus.Suspended, EmployeeStatus.Active) => true,
            (EmployeeStatus.Suspended, EmployeeStatus.Terminated) => true,
            (EmployeeStatus.Terminated, EmployeeStatus.Archived) => true,
            _ => false,
        };

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
