using Fabric.Server.Core;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Contracts;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Kiosk.Endpoints;

public static class KioskEndpoints
{
    public static IEndpointRouteBuilder MapKioskEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder kiosks = app.MapGroup("/api/kiosks");

        kiosks.MapGet("", ListKiosks).Produces<Page<KioskResponse>>();
        kiosks.MapPost("", CreateKiosk).Produces<KioskKeyResponse>(StatusCodes.Status201Created);
        kiosks.MapGet("/{id:guid}", GetKiosk).Produces<KioskResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPut("/{id:guid}", UpdateKiosk).Produces<KioskResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPost("/{id:guid}/rotate-key", RotateKioskKey).Produces<KioskKeyResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapDelete("/{id:guid}", DeleteKiosk).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
        kiosks.MapPost("/{id:guid}/activate", ActivateKiosk).Produces<KioskResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPost("/{id:guid}/maintenance", SetKioskMaintenance).Produces<KioskResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPost("/{id:guid}/disable", DisableKiosk).Produces<KioskResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPut("/{id:guid}/workflow", AssignKioskWorkflow).Produces<KioskResponse>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapGet("/{id:guid}/device-assignments", ListKioskDeviceAssignments).Produces<KioskDeviceAssignmentResponse[]>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPut("/{id:guid}/device-assignments", UpsertKioskDeviceAssignments).Produces<KioskDeviceAssignmentResponse[]>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapGet("/{id:guid}/devices", ListKioskDevices).Produces<KioskDeviceResponse[]>().Produces(StatusCodes.Status404NotFound);
        kiosks.MapPut("/{id:guid}/devices", UpsertKioskDevices).Produces<KioskDeviceResponse[]>().Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListKiosks([AsParameters] BaseListRequest request, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        IPaged<Domain.Kiosk> result = await db.Kiosks.AsNoTracking().OrderBy(kiosk => kiosk.Name).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(kiosk => kiosk.ToResponse()));
    }

    private static async Task<IResult> CreateKiosk([FromBody] CreateKioskRequest request, KioskDbContext db, KioskKeyHasher keyHasher, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        bool profileExists = await db.Profiles.AnyAsync(profile => profile.Id == request.ProfileId, cancellationToken);
        if (!profileExists)
            return Results.Problem("Kiosk profile does not exist.", statusCode: StatusCodes.Status409Conflict);

        KioskKey key = keyHasher.CreateKey();
        Domain.Kiosk kiosk = Domain.Kiosk.Create(request.Name, request.ProfileId, key.Hash, key.Salt, timeProvider.GetUtcNow());
        db.Kiosks.Add(kiosk);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/kiosks/{kiosk.Id}", new KioskKeyResponse(kiosk.ToResponse(), key.Key));
    }

    private static async Task<IResult> GetKiosk(Guid id, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.AsNoTracking().SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        return kiosk is null ? Results.NotFound() : Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> UpdateKiosk(Guid id, [FromBody] UpdateKioskRequest request, KioskDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        if (!await db.Profiles.AnyAsync(profile => profile.Id == request.ProfileId, cancellationToken))
            return Results.Problem("Kiosk profile does not exist.", statusCode: StatusCodes.Status409Conflict);

        kiosk.Update(request.Name, request.ProfileId, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> RotateKioskKey(Guid id, KioskDbContext db, KioskKeyHasher keyHasher, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        KioskKey key = keyHasher.CreateKey();
        kiosk.RotateKey(key.Hash, key.Salt, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(new KioskKeyResponse(kiosk.ToResponse(), key.Key));
    }

    private static async Task<IResult> DeleteKiosk(Guid id, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        await db.Sessions.Where(session => session.KioskId == id).ExecuteDeleteAsync(cancellationToken);
        await db.Devices.Where(device => device.KioskId == id).ExecuteDeleteAsync(cancellationToken);
        await db.DeviceAssignments.Where(assignment => assignment.KioskId == id).ExecuteDeleteAsync(cancellationToken);
        db.Kiosks.Remove(kiosk);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ActivateKiosk(Guid id, KioskDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        kiosk.Activate(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> SetKioskMaintenance(Guid id, KioskDbContext db, KioskSessionCleanupService cleanupService, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        DateTimeOffset now = timeProvider.GetUtcNow();
        kiosk.SetMaintenance(now);
        await CancelActiveSessionsAsync(id, db, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await cleanupService.CleanupAsync(id, cancellationToken);
        return Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> DisableKiosk(Guid id, KioskDbContext db, KioskSessionCleanupService cleanupService, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        DateTimeOffset now = timeProvider.GetUtcNow();
        kiosk.Disable(now);
        await CancelActiveSessionsAsync(id, db, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await cleanupService.CleanupAsync(id, cancellationToken);
        return Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> AssignKioskWorkflow(Guid id, [FromBody] AssignKioskWorkflowRequest request, KioskDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        Domain.Kiosk? kiosk = await db.Kiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        kiosk.AssignWorkflow(request.WorkflowDefinitionId, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> UpsertKioskDeviceAssignments(Guid id, [FromBody] UpsertKioskDeviceAssignmentsRequest request, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Kiosks.AnyAsync(kiosk => kiosk.Id == id, cancellationToken))
            return Results.NotFound();

        await db.DeviceAssignments.Where(assignment => assignment.KioskId == id).ExecuteDeleteAsync(cancellationToken);
        KioskDeviceAssignment[] assignments = request.Assignments.Select(assignment => KioskDeviceAssignment.Create(id, assignment.BindingKey, assignment.AgentId, assignment.DeviceId, assignment.Enabled, assignment.Priority)).ToArray();
        db.DeviceAssignments.AddRange(assignments);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(assignments.Select(assignment => assignment.ToResponse()).ToArray());
    }

    private static async Task<IResult> ListKioskDeviceAssignments(Guid id, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Kiosks.AnyAsync(kiosk => kiosk.Id == id, cancellationToken))
            return Results.NotFound();

        KioskDeviceAssignment[] assignments = await db.DeviceAssignments.AsNoTracking().Where(assignment => assignment.KioskId == id).OrderBy(assignment => assignment.BindingKey).ThenBy(assignment => assignment.Priority).ToArrayAsync(cancellationToken);
        return Results.Ok(assignments.Select(assignment => assignment.ToResponse()).ToArray());
    }

    private static async Task<IResult> UpsertKioskDevices(Guid id, [FromBody] UpsertKioskDevicesRequest request, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Kiosks.AnyAsync(kiosk => kiosk.Id == id, cancellationToken))
            return Results.NotFound();

        if (request.Devices.Any(device => device.SlotNumber < 1))
            return Results.Problem("Kiosk device slot number must be greater than zero.", statusCode: StatusCodes.Status409Conflict);

        if (request.Devices.GroupBy(device => new { device.Type, device.SlotNumber }).Any(group => group.Count() > 1))
            return Results.Problem("Kiosk device slots must be unique per type.", statusCode: StatusCodes.Status409Conflict);

        await db.Devices.Where(device => device.KioskId == id).ExecuteDeleteAsync(cancellationToken);
        KioskDevice[] devices = request.Devices.Select(device => KioskDevice.Create(id, device.Name, device.Type, device.SlotNumber, device.AgentId, device.DeviceId, device.Enabled, device.CleanupOnSessionEnd, device.SortOrder)).ToArray();
        db.Devices.AddRange(devices);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(devices.Select(device => device.ToResponse()).ToArray());
    }

    private static async Task<IResult> ListKioskDevices(Guid id, KioskDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.Kiosks.AnyAsync(kiosk => kiosk.Id == id, cancellationToken))
            return Results.NotFound();

        KioskDevice[] devices = await db.Devices.AsNoTracking().Where(device => device.KioskId == id).OrderBy(device => device.Type).ThenBy(device => device.SlotNumber).ToArrayAsync(cancellationToken);
        return Results.Ok(devices.Select(device => device.ToResponse()).ToArray());
    }

    private static async Task CancelActiveSessionsAsync(Guid kioskId, KioskDbContext db, DateTimeOffset now, CancellationToken cancellationToken)
    {
        KioskSession[] sessions = await db.Sessions.Where(session => session.KioskId == kioskId && session.Status == KioskSessionStatus.Running).ToArrayAsync(cancellationToken);
        foreach (KioskSession session in sessions)
            session.Cancel(now);
    }
}
