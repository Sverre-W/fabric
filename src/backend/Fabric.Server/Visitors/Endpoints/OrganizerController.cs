using Fabric.Server.Core;
using Fabric.Server.Visitors.Contracts;
using Fabric.Server.Visitors.Domain;
using Fabric.Server.Visitors.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Visitors.Endpoints;

[ApiController]
public class OrganizerController
{
    [HttpGet("/api/organizers")]
    [ProducesResponseType(typeof(IPaged<Organizer>), StatusCodes.Status200OK)]
    public async Task<IResult> ListOrganizers(
        [FromQuery] ListOrganizerRequest request,
        [FromServices] VisitorsDbContext db,
        CancellationToken ct
    )
    {
        IQueryable<Organizer> query = db.Organizers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.FirstName, $"%{request.Query}%")
                || EF.Functions.ILike(x.LastName, $"%{request.Query}%")
                || EF.Functions.ILike(x.Email, $"%{request.Query}%")
            );
        }

        IPaged<Organizer> result = await query.GetPageAsync(request.Page, request.PageSize, ct);
        return Results.Ok(result);
    }

    [HttpPost("/api/organizers")]
    [ProducesResponseType(typeof(IPaged<Organizer>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IResult), StatusCodes.Status201Created)]
    public async Task<IResult> CreateOrganizer(
        [FromBody] AddOrganizerRequest request,
        [FromServices] VisitorsDbContext db,
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
        return Results.Created($"/api/organizers/{organizer.Id}", organizer);
    }

    [HttpDelete("/api/organizers/{organizerId}")]
    [ProducesResponseType(typeof(Organizer), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("Deactivate organizer")]
    [EndpointSummary("Deactivate organizer")]
    public async Task<IResult> DeactivateOrganizer(
        Guid organizerId,
        [FromServices] VisitorsDbContext db,
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
}

