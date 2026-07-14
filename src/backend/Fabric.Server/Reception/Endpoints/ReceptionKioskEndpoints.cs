using Fabric.Server.Core;
using Fabric.Server.Reception.Application;
using Fabric.Server.Reception.Contracts;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Endpoints;

public static class ReceptionKioskEndpoints
{
    public static IEndpointRouteBuilder MapReceptionKioskEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder kiosks = app.MapGroup("/api/reception/kiosks");

        kiosks.MapGet("", ListReceptionKiosks)
            .WithDescription("List reception kiosks")
            .WithSummary("List reception kiosks")
            .Produces<Page<ReceptionKioskResponse>>();
        kiosks.MapGet("/{id:guid}", GetReceptionKiosk)
            .WithDescription("Retrieve a reception kiosk")
            .WithSummary("Retrieve reception kiosk")
            .Produces<ReceptionKioskResponse>()
            .Produces(StatusCodes.Status404NotFound);
        kiosks.MapPost("", CreateReceptionKiosk)
            .WithDescription("Create a reception kiosk")
            .WithSummary("Create reception kiosk")
            .Produces<ReceptionKioskKeyResponse>(StatusCodes.Status201Created);
        kiosks.MapPut("/{id:guid}", UpdateReceptionKiosk)
            .WithDescription("Update a reception kiosk")
            .WithSummary("Update reception kiosk")
            .Produces<ReceptionKioskResponse>()
            .Produces(StatusCodes.Status404NotFound);
        kiosks.MapPost("/{id:guid}/rotate-key", RotateReceptionKioskKey)
            .WithDescription("Rotate a reception kiosk API key")
            .WithSummary("Rotate reception kiosk key")
            .Produces<ReceptionKioskKeyResponse>()
            .Produces(StatusCodes.Status404NotFound);
        kiosks.MapDelete("/{id:guid}", DisableReceptionKiosk)
            .WithDescription("Disable a reception kiosk")
            .WithSummary("Disable reception kiosk")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListReceptionKiosks(
        [AsParameters] BaseListRequest request,
        ReceptionDbContext db,
        CancellationToken cancellationToken = default)
    {
        IPaged<ReceptionKiosk> result = await db.ReceptionKiosks
            .AsNoTracking()
            .OrderBy(kiosk => kiosk.Name)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(result.Map(kiosk => kiosk.ToResponse()));
    }

    private static async Task<IResult> GetReceptionKiosk(
        Guid id,
        ReceptionDbContext db,
        CancellationToken cancellationToken = default)
    {
        ReceptionKiosk? kiosk = await db.ReceptionKiosks
            .AsNoTracking()
            .SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);

        return kiosk is null ? Results.NotFound() : Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> CreateReceptionKiosk(
        [FromBody] CreateReceptionKioskRequest request,
        ReceptionDbContext db,
        ReceptionKioskKeyHasher keyHasher,
        CancellationToken cancellationToken = default)
    {
        ReceptionKioskKey key = keyHasher.CreateKey();
        ReceptionKiosk kiosk = ReceptionKiosk.Create(
            request.Name,
            request.LocationId,
            key.Hash,
            key.Salt,
            request.RequireFacePicture,
            request.IdentityVerificationMethod);

        db.ReceptionKiosks.Add(kiosk);
        await db.SaveChangesAsync(cancellationToken);

        var response = new ReceptionKioskKeyResponse(kiosk.ToResponse(), key.Key);
        return Results.Created($"/api/reception/kiosks/{kiosk.Id}", response);
    }

    private static async Task<IResult> UpdateReceptionKiosk(
        Guid id,
        [FromBody] UpdateReceptionKioskRequest request,
        ReceptionDbContext db,
        CancellationToken cancellationToken = default)
    {
        ReceptionKiosk? kiosk = await db.ReceptionKiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        kiosk.Update(
            request.Name,
            request.LocationId,
            request.Enabled,
            request.RequireFacePicture,
            request.IdentityVerificationMethod);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(kiosk.ToResponse());
    }

    private static async Task<IResult> RotateReceptionKioskKey(
        Guid id,
        ReceptionDbContext db,
        ReceptionKioskKeyHasher keyHasher,
        CancellationToken cancellationToken = default)
    {
        ReceptionKiosk? kiosk = await db.ReceptionKiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        ReceptionKioskKey key = keyHasher.CreateKey();
        kiosk.RotateKey(key.Hash, key.Salt);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new ReceptionKioskKeyResponse(kiosk.ToResponse(), key.Key));
    }

    private static async Task<IResult> DisableReceptionKiosk(
        Guid id,
        ReceptionDbContext db,
        CancellationToken cancellationToken = default)
    {
        ReceptionKiosk? kiosk = await db.ReceptionKiosks.SingleOrDefaultAsync(kiosk => kiosk.Id == id, cancellationToken);
        if (kiosk is null)
            return Results.NotFound();

        kiosk.Disable();
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}
