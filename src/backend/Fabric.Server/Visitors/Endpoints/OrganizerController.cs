using Fabric.Server.Core;
using Fabric.Server.Visitors.Contracts;
using Fabric.Server.Visitors.Domain;
using Fabric.Server.Visitors.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Visitors.Endpoints;

public static class OrganizerEndpoints
{
    public static IEndpointRouteBuilder MapOrganizerEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/visitors/organizers");

        group.MapGet("", ListOrganizers)
            .Produces<Page<OrganizerResponse>>();
        group.MapGet("/{organizerId:guid}", GetOrganizer)
            .Produces<OrganizerResponse>()
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost("", CreateOrganizer)
            .Produces<OrganizerResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        group.MapDelete("/{organizerId:guid}", DeactivateOrganizer)
            .WithDescription("Deactivate organizer")
            .WithSummary("Deactivate organizer")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        group.MapPut("/{organizerId:guid}", UpdateOrganizer)
            .Produces<OrganizerResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> ListOrganizers(
        [AsParameters] ListOrganizerRequest request,
        VisitorsDbContext db,
        CancellationToken ct
    )
    {
        IQueryable<Organizer> query = db.Organizers.AsNoTracking().Where(x => x.Active);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.FirstName, $"%{request.Query}%")
                || EF.Functions.ILike(x.LastName, $"%{request.Query}%")
                || EF.Functions.ILike(x.Email, $"%{request.Query}%")
            );
        }

        IPaged<Organizer> result = await query.GetPageAsync(request.Page, request.PageSize, ct);
        return Results.Ok(result.Map(ToResponse));
    }

    private static async Task<IResult> GetOrganizer(
        Guid organizerId,
        VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        Organizer? organizer = await db.Organizers.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == organizerId && x.Active,
            cancellationToken
        );

        return organizer is null ? Results.NotFound() : Results.Ok(ToResponse(organizer));
    }

    private static async Task<IResult> CreateOrganizer(
        [FromBody] AddOrganizerRequest request,
        VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        Organizer? organizer = await db.Organizers.SingleOrDefaultAsync(
            x => x.Email == request.Email,
            cancellationToken
        );

        if (organizer is null)
        {
            organizer = Organizer.Create(request.FirstName, request.LastName, request.Email);
            db.Organizers.Add(organizer);
        }
        else
        {
            if (organizer.Active)
            {
                return Results.Conflict();
            }

            organizer.Activate();
        }

        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/visitors/organizers/{organizer.Id}", ToResponse(organizer));
    }

    private static async Task<IResult> DeactivateOrganizer(
        Guid organizerId,
        VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        Organizer? organizer = await db.Organizers.SingleOrDefaultAsync(
            x => x.Id == organizerId && x.Active == true,
            cancellationToken
        );

        if (organizer is null)
            return Results.NotFound();

        organizer.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateOrganizer(
        Guid organizerId,
        [FromBody] UpdateOrganizerRequest request,
        VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        Organizer? organizer = await db.Organizers.SingleOrDefaultAsync(
            x => x.Id == organizerId && x.Active,
            cancellationToken
        );

        if (organizer is null)
            return Results.NotFound();

        bool emailExists = await db.Organizers.AnyAsync(
            x => x.Id != organizerId && x.Email == request.Email && x.Active,
            cancellationToken
        );

        if (emailExists)
            return Results.Conflict();

        organizer.UpdateProfile(request.FirstName, request.LastName, request.Email);

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(organizer));
    }

    private static OrganizerResponse ToResponse(Organizer organizer) =>
        new(organizer.Id, organizer.FirstName, organizer.LastName, organizer.Email);
}
