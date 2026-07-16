using Fabric.Server.Core;
using Fabric.Server.Employees.Contracts;
using Fabric.Server.Employees.Domain;
using Fabric.Server.Employees.Persistence;
using Fabric.Server.Identities.Domain;
using Fabric.Server.Identities.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Fabric.Server.Employees.Application;

public sealed class EmployeeService(
    EmployeesDbContext db,
    IdentitiesDbContext identitiesDb,
    TimeProvider timeProvider)
{
    public async Task<Result<(Employee Employee, OrganizationUnit OrganizationUnit, Identity Identity), EmployeeErrors>> CreateEmployeeAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        OrganizationUnit? organizationUnit = await db.OrganizationUnits.SingleOrDefaultAsync(unit => unit.Id == request.OrganizationUnitId, cancellationToken);
        if (organizationUnit is null)
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

        if (!organizationUnit.IsActive)
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(EmployeeErrors.OrganizationUnitInactive);

        if (request.ManagerEmployeeId.HasValue && !await db.Employees.AnyAsync(employee => employee.Id == request.ManagerEmployeeId.Value, cancellationToken))
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(EmployeeErrors.ManagerNotFound);

        string? employeeNumber = NormalizeOptional(request.EmployeeNumber);
        if (employeeNumber is not null && await db.Employees.AnyAsync(employee => employee.EmployeeNumber == employeeNumber, cancellationToken))
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(EmployeeErrors.EmployeeNumberAlreadyExists);

        Result<Identity, IdentityErrors> createIdentity = Identity.Create(
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.PreferredName,
            request.Email,
            request.Phone,
            now);
        if (createIdentity.IsFailure(out _))
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(EmployeeErrors.IdentityCreationFailed);

        createIdentity.IsSuccess(out Identity identity);

        Result<Employee, EmployeeErrors> createEmployee = Employee.Create(
            identity.Id,
            request.OrganizationUnitId,
            request.ManagerEmployeeId,
            employeeNumber,
            request.JobTitle,
            request.Status ?? EmployeeStatus.PreHire,
            request.HireDate,
            now);
        if (createEmployee.IsFailure(out EmployeeErrors error))
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(error);

        createEmployee.IsSuccess(out Employee employee);
        Result<EmployeeAffiliation, IdentityErrors> addAffiliation = identity.AddEmployeeAffiliation(employee.Id, now, null, now);
        if (addAffiliation.IsFailure(out _))
            return Result.Failure<(Employee, OrganizationUnit, Identity), EmployeeErrors>(EmployeeErrors.IdentityCreationFailed);

        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await identitiesDb.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);

        identitiesDb.Identities.Add(identity);
        db.Employees.Add(employee);

        await identitiesDb.SaveChangesAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success<(Employee, OrganizationUnit, Identity), EmployeeErrors>((employee, organizationUnit, identity));
    }

    public async Task<Result<(Employee Employee, OrganizationUnit OrganizationUnit), EmployeeErrors>> UpdateWorkDetailsAsync(
        Guid employeeId,
        UpdateEmployeeWorkDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        Employee? employee = await db.Employees.SingleOrDefaultAsync(item => item.Id == employeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.EmployeeNotFound);

        OrganizationUnit? organizationUnit = await db.OrganizationUnits.SingleOrDefaultAsync(unit => unit.Id == request.OrganizationUnitId, cancellationToken);
        if (organizationUnit is null)
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

        if (!organizationUnit.IsActive)
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.OrganizationUnitInactive);

        if (request.ManagerEmployeeId == employee.Id)
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.ManagerCannotBeSelf);

        if (request.ManagerEmployeeId.HasValue && !await db.Employees.AnyAsync(item => item.Id == request.ManagerEmployeeId.Value, cancellationToken))
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.ManagerNotFound);

        string? employeeNumber = NormalizeOptional(request.EmployeeNumber);
        if (employeeNumber is not null
            && await db.Employees.AnyAsync(item => item.Id != employee.Id && item.EmployeeNumber == employeeNumber, cancellationToken))
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.EmployeeNumberAlreadyExists);

        Result<EmployeeErrors> update = employee.UpdateWorkDetails(
            request.OrganizationUnitId,
            request.ManagerEmployeeId,
            employeeNumber,
            request.JobTitle,
            request.HireDate,
            timeProvider.GetUtcNow());
        if (update.IsFailure(out EmployeeErrors error))
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(error);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<(Employee, OrganizationUnit), EmployeeErrors>((employee, organizationUnit));
    }

    public async Task<Result<(Employee Employee, OrganizationUnit OrganizationUnit), EmployeeErrors>> TransitionStatusAsync(
        Guid employeeId,
        EmployeeStatus status,
        DateOnly? effectiveDate,
        CancellationToken cancellationToken = default)
    {
        Employee? employee = await db.Employees.SingleOrDefaultAsync(item => item.Id == employeeId, cancellationToken);
        if (employee is null)
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.EmployeeNotFound);

        OrganizationUnit organizationUnit = await db.OrganizationUnits.SingleAsync(unit => unit.Id == employee.OrganizationUnitId, cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();

        Result<EmployeeErrors> transition = employee.TransitionTo(status, effectiveDate, now);
        if (transition.IsFailure(out EmployeeErrors error))
            return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(error);

        if (status == EmployeeStatus.Terminated)
        {
            EmployeeAffiliation? affiliation = await identitiesDb.EmployeeAffiliations
                .SingleOrDefaultAsync(item => item.EmployeeId == employee.Id, cancellationToken);
            Result<IdentityErrors> endAffiliation = affiliation?.End(now) ?? Result.Success<IdentityErrors>();
            if (endAffiliation.IsFailure(out _))
                return Result.Failure<(Employee, OrganizationUnit), EmployeeErrors>(EmployeeErrors.InvalidEmployeeStatusTransition);

            await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await identitiesDb.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
            await identitiesDb.SaveChangesAsync(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        else
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success<(Employee, OrganizationUnit), EmployeeErrors>((employee, organizationUnit));
    }

    public async Task<Result<OrganizationUnit, EmployeeErrors>> CreateOrganizationUnitAsync(
        CreateOrganizationUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        OrganizationUnit? parent = null;
        if (request.ParentId.HasValue)
        {
            parent = await db.OrganizationUnits.SingleOrDefaultAsync(unit => unit.Id == request.ParentId.Value, cancellationToken);
            if (parent is null)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

            if (!parent.IsActive)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitInactive);
        }

        if (request.Code is not null && await db.OrganizationUnits.AnyAsync(unit => unit.Code == request.Code, cancellationToken))
            return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitAlreadyExists);

        Result<OrganizationUnit, EmployeeErrors> create = OrganizationUnit.Create(request.Name, request.Code, request.Type, request.ParentId, now);
        if (create.IsFailure(out EmployeeErrors error))
            return Result.Failure<OrganizationUnit, EmployeeErrors>(error);

        create.IsSuccess(out OrganizationUnit unit);
        db.OrganizationUnits.Add(unit);
        await AddClosureRowsForNewUnitAsync(unit, parent?.Id, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<OrganizationUnit, EmployeeErrors>(unit);
    }

    public async Task<Result<OrganizationUnit, EmployeeErrors>> UpdateOrganizationUnitAsync(
        Guid id,
        UpdateOrganizationUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        OrganizationUnit? unit = await db.OrganizationUnits.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (unit is null)
            return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

        if (request.Code is not null
            && await db.OrganizationUnits.AnyAsync(item => item.Id != id && item.Code == request.Code, cancellationToken))
            return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitAlreadyExists);

        Result<EmployeeErrors> update = unit.Update(request.Name, request.Code, request.Type, timeProvider.GetUtcNow());
        if (update.IsFailure(out EmployeeErrors error))
            return Result.Failure<OrganizationUnit, EmployeeErrors>(error);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<OrganizationUnit, EmployeeErrors>(unit);
    }

    public async Task<Result<OrganizationUnit, EmployeeErrors>> MoveOrganizationUnitAsync(
        Guid id,
        Guid? parentId,
        CancellationToken cancellationToken = default)
    {
        OrganizationUnit? unit = await db.OrganizationUnits.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (unit is null)
            return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

        if (parentId.HasValue)
        {
            OrganizationUnit? parent = await db.OrganizationUnits.SingleOrDefaultAsync(item => item.Id == parentId.Value, cancellationToken);
            if (parent is null)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

            if (!parent.IsActive)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitInactive);

            bool parentIsDescendant = await db.OrganizationUnitClosures
                .AnyAsync(closure => closure.AncestorId == id && closure.DescendantId == parentId.Value, cancellationToken);
            if (parentIsDescendant)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitParentCycle);
        }

        Result<EmployeeErrors> move = unit.Move(parentId, timeProvider.GetUtcNow());
        if (move.IsFailure(out EmployeeErrors error))
            return Result.Failure<OrganizationUnit, EmployeeErrors>(error);

        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await RebuildClosureRowsForMoveAsync(id, parentId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success<OrganizationUnit, EmployeeErrors>(unit);
    }

    public async Task<Result<OrganizationUnit, EmployeeErrors>> SetOrganizationUnitActiveAsync(
        Guid id,
        bool active,
        CancellationToken cancellationToken = default)
    {
        OrganizationUnit? unit = await db.OrganizationUnits.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (unit is null)
            return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitNotFound);

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (active)
        {
            unit.Activate(now);
        }
        else
        {
            bool hasActiveChildren = await db.OrganizationUnits.AnyAsync(item => item.ParentId == id && item.IsActive, cancellationToken);
            if (hasActiveChildren)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitHasActiveChildren);

            Guid[] subtreeIds = await GetSubtreeIdsAsync(id, cancellationToken);
            bool hasActiveEmployees = await db.Employees
                .AnyAsync(employee => subtreeIds.Contains(employee.OrganizationUnitId)
                    && employee.Status != EmployeeStatus.Terminated
                    && employee.Status != EmployeeStatus.Archived, cancellationToken);
            if (hasActiveEmployees)
                return Result.Failure<OrganizationUnit, EmployeeErrors>(EmployeeErrors.OrganizationUnitHasActiveEmployees);

            unit.Deactivate(now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success<OrganizationUnit, EmployeeErrors>(unit);
    }

    private async Task AddClosureRowsForNewUnitAsync(OrganizationUnit unit, Guid? parentId, CancellationToken cancellationToken)
    {
        db.OrganizationUnitClosures.Add(OrganizationUnitClosure.Create(unit.Id, unit.Id, 0));

        if (!parentId.HasValue)
            return;

        List<OrganizationUnitClosure> ancestors = await db.OrganizationUnitClosures
            .Where(closure => closure.DescendantId == parentId.Value)
            .ToListAsync(cancellationToken);

        foreach (OrganizationUnitClosure ancestor in ancestors)
        {
            db.OrganizationUnitClosures.Add(OrganizationUnitClosure.Create(
                ancestor.AncestorId,
                unit.Id,
                ancestor.Depth + 1));
        }
    }

    private async Task RebuildClosureRowsForMoveAsync(Guid unitId, Guid? parentId, CancellationToken cancellationToken)
    {
        List<OrganizationUnitClosure> subtree = await db.OrganizationUnitClosures
            .Where(closure => closure.AncestorId == unitId)
            .ToListAsync(cancellationToken);
        Guid[] subtreeIds = subtree.Select(closure => closure.DescendantId).ToArray();

        await db.OrganizationUnitClosures
            .Where(closure => subtreeIds.Contains(closure.DescendantId) && !subtreeIds.Contains(closure.AncestorId))
            .ExecuteDeleteAsync(cancellationToken);

        if (!parentId.HasValue)
            return;

        List<OrganizationUnitClosure> parentAncestors = await db.OrganizationUnitClosures
            .Where(closure => closure.DescendantId == parentId.Value)
            .ToListAsync(cancellationToken);

        foreach (OrganizationUnitClosure ancestor in parentAncestors)
        {
            foreach (OrganizationUnitClosure descendant in subtree)
            {
                db.OrganizationUnitClosures.Add(OrganizationUnitClosure.Create(
                    ancestor.AncestorId,
                    descendant.DescendantId,
                    ancestor.Depth + descendant.Depth + 1));
            }
        }
    }

    private async Task<Guid[]> GetSubtreeIdsAsync(Guid organizationUnitId, CancellationToken cancellationToken) =>
        await db.OrganizationUnitClosures
            .Where(closure => closure.AncestorId == organizationUnitId)
            .Select(closure => closure.DescendantId)
            .ToArrayAsync(cancellationToken);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
