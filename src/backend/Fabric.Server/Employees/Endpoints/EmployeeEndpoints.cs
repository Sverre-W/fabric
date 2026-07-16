using Fabric.Server.Core;
using Fabric.Server.Employees.Application;
using Fabric.Server.Employees.Contracts;
using Fabric.Server.Employees.Domain;
using Fabric.Server.Employees.Persistence;
using Fabric.Server.Identities.Domain;
using Fabric.Server.Identities.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Employees.Endpoints;

public static class EmployeeEndpoints
{
    public static IEndpointRouteBuilder MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder employees = app.MapGroup("/api/employees/employees");

        employees.MapGet("", ListEmployees)
            .WithSummary("List employees")
            .WithDescription("List employees with optional search, status, and organization unit filters.")
            .Produces<Page<EmployeeResponse>>();
        employees.MapGet("/{id:guid}", GetEmployee)
            .WithSummary("Get employee")
            .WithDescription("Get one employee.")
            .Produces<EmployeeResponse>()
            .Produces(StatusCodes.Status404NotFound);
        employees.MapPost("", CreateEmployee)
            .WithSummary("Create employee")
            .WithDescription("Create an employee and canonical identity.")
            .Produces<EmployeeResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        employees.MapPut("/{id:guid}/work-details", UpdateWorkDetails)
            .WithSummary("Update employee work details")
            .WithDescription("Update employee number, organization unit, manager, title, and hire date.")
            .Produces<EmployeeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        employees.MapPost("/{id:guid}/status", TransitionStatus)
            .WithSummary("Transition employee status")
            .WithDescription("Move an employee through the employment lifecycle.")
            .Produces<EmployeeResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        RouteGroupBuilder organizationUnits = app.MapGroup("/api/employees/organization-units");

        organizationUnits.MapGet("", ListOrganizationUnits)
            .WithSummary("List organization units")
            .WithDescription("List organization units.")
            .Produces<Page<OrganizationUnitResponse>>();
        organizationUnits.MapGet("/{id:guid}", GetOrganizationUnit)
            .WithSummary("Get organization unit")
            .WithDescription("Get one organization unit.")
            .Produces<OrganizationUnitResponse>()
            .Produces(StatusCodes.Status404NotFound);
        organizationUnits.MapGet("/{id:guid}/ancestors", ListAncestors)
            .WithSummary("List organization unit ancestors")
            .WithDescription("List ancestors for an organization unit.")
            .Produces<OrganizationUnitResponse[]>();
        organizationUnits.MapGet("/{id:guid}/descendants", ListDescendants)
            .WithSummary("List organization unit descendants")
            .WithDescription("List descendants for an organization unit.")
            .Produces<OrganizationUnitResponse[]>();
        organizationUnits.MapGet("/{id:guid}/employees", ListSubtreeEmployees)
            .WithSummary("List organization unit subtree employees")
            .WithDescription("List employees assigned to an organization unit subtree.")
            .Produces<Page<EmployeeResponse>>();
        organizationUnits.MapPost("", CreateOrganizationUnit)
            .WithSummary("Create organization unit")
            .WithDescription("Create an organization unit.")
            .Produces<OrganizationUnitResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
        organizationUnits.MapPut("/{id:guid}", UpdateOrganizationUnit)
            .WithSummary("Update organization unit")
            .WithDescription("Update organization unit details.")
            .Produces<OrganizationUnitResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        organizationUnits.MapPost("/{id:guid}/move", MoveOrganizationUnit)
            .WithSummary("Move organization unit")
            .WithDescription("Move an organization unit under a different parent.")
            .Produces<OrganizationUnitResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        organizationUnits.MapPost("/{id:guid}/activate", ActivateOrganizationUnit)
            .WithSummary("Activate organization unit")
            .WithDescription("Activate an organization unit.")
            .Produces<OrganizationUnitResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
        organizationUnits.MapPost("/{id:guid}/deactivate", DeactivateOrganizationUnit)
            .WithSummary("Deactivate organization unit")
            .WithDescription("Deactivate an organization unit.")
            .Produces<OrganizationUnitResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> ListEmployees(
        [AsParameters] ListEmployeesRequest request,
        EmployeesDbContext db,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Employee> query = db.Employees.AsNoTracking();

        if (request.Status is { Length: > 0 })
            query = query.Where(employee => request.Status.Contains(employee.Status));

        if (request.OrganizationUnitId.HasValue)
        {
            if (request.IncludeDescendants)
            {
                Guid[] unitIds = await db.OrganizationUnitClosures
                    .Where(closure => closure.AncestorId == request.OrganizationUnitId.Value)
                    .Select(closure => closure.DescendantId)
                    .ToArrayAsync(cancellationToken);
                query = query.Where(employee => unitIds.Contains(employee.OrganizationUnitId));
            }
            else
            {
                query = query.Where(employee => employee.OrganizationUnitId == request.OrganizationUnitId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            string filter = $"%{request.Query}%";
            query = query.Where(employee =>
                employee.EmployeeNumber != null && EF.Functions.ILike(employee.EmployeeNumber, filter)
                || employee.JobTitle != null && EF.Functions.ILike(employee.JobTitle, filter));
        }

        IPaged<Employee> page = await query
            .OrderBy(employee => employee.EmployeeNumber)
            .ThenBy(employee => employee.Id)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        Page<EmployeeResponse> response = await MapEmployeePageAsync(page, db, identitiesDb, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetEmployee(
        Guid id,
        EmployeesDbContext db,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken = default)
    {
        Employee? employee = await db.Employees.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (employee is null)
            return Results.NotFound();

        EmployeeResponse response = await MapEmployeeAsync(employee, db, identitiesDb, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        EmployeeService employeeService,
        CancellationToken cancellationToken = default)
    {
        Result<(Employee Employee, OrganizationUnit OrganizationUnit, Identity Identity), EmployeeErrors> result =
            await employeeService.CreateEmployeeAsync(request, cancellationToken);

        return result.Match<IResult>(
            value => Results.Created($"/api/employees/employees/{value.Employee.Id}", value.Employee.ToResponse(value.OrganizationUnit, value.Identity)),
            error => ToResult(MapError(error)));
    }

    private static async Task<IResult> UpdateWorkDetails(
        Guid id,
        [FromBody] UpdateEmployeeWorkDetailsRequest request,
        EmployeeService employeeService,
        EmployeesDbContext db,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken = default)
    {
        Result<(Employee Employee, OrganizationUnit OrganizationUnit), EmployeeErrors> result =
            await employeeService.UpdateWorkDetailsAsync(id, request, cancellationToken);

        if (result.IsFailure(out EmployeeErrors error))
            return ToResult(MapError(error));

        result.IsSuccess(out (Employee Employee, OrganizationUnit OrganizationUnit) value);
        Identity? identity = await identitiesDb.Identities.AsNoTracking().SingleOrDefaultAsync(item => item.Id == value.Employee.IdentityId, cancellationToken);
        return Results.Ok(value.Employee.ToResponse(value.OrganizationUnit, identity));
    }

    private static async Task<IResult> TransitionStatus(
        Guid id,
        [FromBody] TransitionEmployeeStatusRequest request,
        EmployeeService employeeService,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken = default)
    {
        Result<(Employee Employee, OrganizationUnit OrganizationUnit), EmployeeErrors> result =
            await employeeService.TransitionStatusAsync(id, request.Status, request.EffectiveDate, cancellationToken);

        if (result.IsFailure(out EmployeeErrors error))
            return ToResult(MapError(error));

        result.IsSuccess(out (Employee Employee, OrganizationUnit OrganizationUnit) value);
        Identity? identity = await identitiesDb.Identities.AsNoTracking().SingleOrDefaultAsync(item => item.Id == value.Employee.IdentityId, cancellationToken);
        return Results.Ok(value.Employee.ToResponse(value.OrganizationUnit, identity));
    }

    private static async Task<IResult> ListOrganizationUnits(
        [AsParameters] ListOrganizationUnitsRequest request,
        EmployeesDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OrganizationUnit> query = db.OrganizationUnits.AsNoTracking();

        if (request.ParentId.HasValue)
            query = query.Where(unit => unit.ParentId == request.ParentId.Value);

        if (request.IsActive.HasValue)
            query = query.Where(unit => unit.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            string filter = $"%{request.Query}%";
            query = query.Where(unit =>
                EF.Functions.ILike(unit.Name, filter)
                || unit.Code != null && EF.Functions.ILike(unit.Code, filter)
                || EF.Functions.ILike(unit.Type, filter));
        }

        IPaged<OrganizationUnit> page = await query
            .OrderBy(unit => unit.Name)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(page.Map(unit => unit.ToResponse()));
    }

    private static async Task<IResult> GetOrganizationUnit(
        Guid id,
        EmployeesDbContext db,
        CancellationToken cancellationToken = default)
    {
        OrganizationUnit? unit = await db.OrganizationUnits.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (unit is null)
            return Results.NotFound();

        int depth = await GetUnitDepthAsync(id, db, cancellationToken);
        int childCount = await db.OrganizationUnits.CountAsync(item => item.ParentId == id, cancellationToken);
        int employeeCount = await db.Employees.CountAsync(item => item.OrganizationUnitId == id, cancellationToken);
        return Results.Ok(unit.ToResponse(depth, childCount, employeeCount));
    }

    private static async Task<IResult> ListAncestors(
        Guid id,
        EmployeesDbContext db,
        CancellationToken cancellationToken = default)
    {
        var closures = await db.OrganizationUnitClosures
            .AsNoTracking()
            .Where(closure => closure.DescendantId == id && closure.Depth > 0)
            .OrderByDescending(closure => closure.Depth)
            .ToListAsync(cancellationToken);
        Guid[] ids = closures.Select(closure => closure.AncestorId).ToArray();
        Dictionary<Guid, int> depths = closures.ToDictionary(closure => closure.AncestorId, closure => closure.Depth);
        List<OrganizationUnit> units = await db.OrganizationUnits.AsNoTracking().Where(unit => ids.Contains(unit.Id)).ToListAsync(cancellationToken);
        OrganizationUnitResponse[] response = units
            .OrderBy(unit => depths[unit.Id])
            .Select(unit => unit.ToResponse(depths[unit.Id]))
            .ToArray();
        return Results.Ok(response);
    }

    private static async Task<IResult> ListDescendants(
        Guid id,
        EmployeesDbContext db,
        CancellationToken cancellationToken = default)
    {
        var closures = await db.OrganizationUnitClosures
            .AsNoTracking()
            .Where(closure => closure.AncestorId == id && closure.Depth > 0)
            .OrderBy(closure => closure.Depth)
            .ToListAsync(cancellationToken);
        Guid[] ids = closures.Select(closure => closure.DescendantId).ToArray();
        Dictionary<Guid, int> depths = closures.ToDictionary(closure => closure.DescendantId, closure => closure.Depth);
        List<OrganizationUnit> units = await db.OrganizationUnits.AsNoTracking().Where(unit => ids.Contains(unit.Id)).ToListAsync(cancellationToken);
        OrganizationUnitResponse[] response = units
            .OrderBy(unit => depths[unit.Id])
            .ThenBy(unit => unit.Name)
            .Select(unit => unit.ToResponse(depths[unit.Id]))
            .ToArray();
        return Results.Ok(response);
    }

    private static async Task<IResult> ListSubtreeEmployees(
        Guid id,
        [AsParameters] ListEmployeesRequest request,
        EmployeesDbContext db,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken = default)
    {
        request.OrganizationUnitId = id;
        request.IncludeDescendants = true;
        return await ListEmployees(request, db, identitiesDb, cancellationToken);
    }

    private static async Task<IResult> CreateOrganizationUnit(
        [FromBody] CreateOrganizationUnitRequest request,
        EmployeeService employeeService,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationUnit, EmployeeErrors> result = await employeeService.CreateOrganizationUnitAsync(request, cancellationToken);
        return result.Match<IResult>(
            unit => Results.Created($"/api/employees/organization-units/{unit.Id}", unit.ToResponse()),
            error => ToResult(MapError(error)));
    }

    private static async Task<IResult> UpdateOrganizationUnit(
        Guid id,
        [FromBody] UpdateOrganizationUnitRequest request,
        EmployeeService employeeService,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationUnit, EmployeeErrors> result = await employeeService.UpdateOrganizationUnitAsync(id, request, cancellationToken);
        return result.Map(unit => unit.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> MoveOrganizationUnit(
        Guid id,
        [FromBody] MoveOrganizationUnitRequest request,
        EmployeeService employeeService,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationUnit, EmployeeErrors> result = await employeeService.MoveOrganizationUnitAsync(id, request.ParentId, cancellationToken);
        return result.Map(unit => unit.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> ActivateOrganizationUnit(
        Guid id,
        EmployeeService employeeService,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationUnit, EmployeeErrors> result = await employeeService.SetOrganizationUnitActiveAsync(id, true, cancellationToken);
        return result.Map(unit => unit.ToResponse()).AsResponse(MapError);
    }

    private static async Task<IResult> DeactivateOrganizationUnit(
        Guid id,
        EmployeeService employeeService,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationUnit, EmployeeErrors> result = await employeeService.SetOrganizationUnitActiveAsync(id, false, cancellationToken);
        return result.Map(unit => unit.ToResponse()).AsResponse(MapError);
    }

    private static async Task<Page<EmployeeResponse>> MapEmployeePageAsync(
        IPaged<Employee> page,
        EmployeesDbContext db,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken)
    {
        Guid[] organizationUnitIds = page.Items.Select(employee => employee.OrganizationUnitId).Distinct().ToArray();
        Guid[] identityIds = page.Items.Select(employee => employee.IdentityId).Distinct().ToArray();

        Dictionary<Guid, OrganizationUnit> organizationUnits = await db.OrganizationUnits.AsNoTracking()
            .Where(unit => organizationUnitIds.Contains(unit.Id))
            .ToDictionaryAsync(unit => unit.Id, cancellationToken);
        Dictionary<Guid, Identity> identities = await identitiesDb.Identities.AsNoTracking()
            .Where(identity => identityIds.Contains(identity.Id))
            .ToDictionaryAsync(identity => identity.Id, cancellationToken);

        return page.Map(employee => employee.ToResponse(
            organizationUnits[employee.OrganizationUnitId],
            identities.GetValueOrDefault(employee.IdentityId)));
    }

    private static async Task<EmployeeResponse> MapEmployeeAsync(
        Employee employee,
        EmployeesDbContext db,
        IdentitiesDbContext identitiesDb,
        CancellationToken cancellationToken)
    {
        OrganizationUnit organizationUnit = await db.OrganizationUnits.AsNoTracking()
            .SingleAsync(unit => unit.Id == employee.OrganizationUnitId, cancellationToken);
        Identity? identity = await identitiesDb.Identities.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == employee.IdentityId, cancellationToken);
        return employee.ToResponse(organizationUnit, identity);
    }

    private static async Task<int> GetUnitDepthAsync(Guid id, EmployeesDbContext db, CancellationToken cancellationToken)
    {
        int? depth = await db.OrganizationUnitClosures
            .Where(closure => closure.DescendantId == id)
            .MaxAsync(closure => (int?)closure.Depth, cancellationToken);
        return depth ?? 0;
    }

    private static (int statusCode, ProblemDetails? problemDetails) MapError(EmployeeErrors error) =>
        error switch
        {
            EmployeeErrors.EmployeeNotFound => Problem(StatusCodes.Status404NotFound, "Employee not found."),
            EmployeeErrors.OrganizationUnitNotFound => Problem(StatusCodes.Status404NotFound, "Organization unit not found."),
            EmployeeErrors.ManagerNotFound => Problem(StatusCodes.Status404NotFound, "Manager employee not found."),
            EmployeeErrors.OrganizationUnitInactive => Problem(StatusCodes.Status409Conflict, "Organization unit is inactive."),
            EmployeeErrors.OrganizationUnitAlreadyExists => Problem(StatusCodes.Status409Conflict, "Organization unit already exists."),
            EmployeeErrors.OrganizationUnitHasActiveChildren => Problem(StatusCodes.Status409Conflict, "Organization unit has active children."),
            EmployeeErrors.OrganizationUnitHasActiveEmployees => Problem(StatusCodes.Status409Conflict, "Organization unit has active employees."),
            EmployeeErrors.OrganizationUnitParentCycle => Problem(StatusCodes.Status409Conflict, "Organization unit move would create a cycle."),
            EmployeeErrors.EmployeeNumberAlreadyExists => Problem(StatusCodes.Status409Conflict, "Employee number already exists."),
            EmployeeErrors.ManagerCannotBeSelf => Problem(StatusCodes.Status400BadRequest, "Manager employee cannot be the employee itself."),
            EmployeeErrors.InvalidEmployeeStatusTransition => Problem(StatusCodes.Status409Conflict, "Invalid employee status transition."),
            EmployeeErrors.HireDateRequiredForActiveEmployee => Problem(StatusCodes.Status400BadRequest, "Hire date is required for active employees."),
            EmployeeErrors.TerminationDateRequired => Problem(StatusCodes.Status400BadRequest, "Termination date is required."),
            EmployeeErrors.EmployeeAlreadyArchived => Problem(StatusCodes.Status409Conflict, "Employee is already archived."),
            EmployeeErrors.IdentityCreationFailed => Problem(StatusCodes.Status400BadRequest, "Identity could not be created."),
            _ => Problem(StatusCodes.Status400BadRequest, "Employee request is invalid."),
        };

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) =>
        (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });

    private static IResult ToResult((int statusCode, ProblemDetails? problemDetails) error) =>
        error.problemDetails is null
            ? Results.StatusCode(error.statusCode)
            : Results.Json(error.problemDetails, statusCode: error.statusCode);
}
