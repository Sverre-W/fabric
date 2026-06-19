using Fabric.Server.Core;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Fabric.Server.Visitors.Application;
using Fabric.Server.Visitors.Contracts;
using Fabric.Server.Visitors.Domain;
using Fabric.Server.Visitors.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Visitors.Endpoints;

[ApiController]
public class VisitorsController
{
    [HttpGet("/api/visitors/visitors")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<VisitorResponse>))]
    [EndpointDescription("List visitors")]
    [EndpointSummary("List visitors")]
    public async Task<IResult> ListVisitors(
        [FromQuery] ListVisitorsRequest request,
        [FromQuery] Guid[]? ids,
        [FromServices] VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Visitor> query = db.Visitors.AsNoTracking();

        if (ids is { Length: > 0 })
            query = query.Where(visitor => ids.Contains(visitor.Id));

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            string filter = $"%{request.Query}%";
            query = query.Where(visitor =>
                EF.Functions.ILike(visitor.FirstName, filter)
                || EF.Functions.ILike(visitor.LastName, filter)
                || EF.Functions.ILike(visitor.Email, filter)
                || EF.Functions.ILike(visitor.Company!, filter)
            );
        }

        IPaged<Visitor> result = await query
            .OrderBy(visitor => visitor.LastName)
            .ThenBy(visitor => visitor.FirstName)
            .GetPageAsync(request.Page, request.PageSize, cancellationToken);

        return Results.Ok(result.Map(visitor => visitor.ToResponse()));
    }

    [HttpGet("/api/visitors/visits/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VisitResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("Retrieve a visit by id")]
    [EndpointSummary("Retrieve a visit by id")]
    public async Task<IResult> GetVisitById(
        [FromServices] VisitorsDbContext db,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        Visit? visitRow = await db
            .Visits.Include(x => x.Invitations)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (visitRow is null)
            return Results.NotFound();

        Organizer organizer = await db.Organizers.SingleAsync(
            x => x.Id == visitRow.OrganizerId,
            cancellationToken
        );

        return Results.Ok(visitRow.ToResponse(organizer));
    }

    [HttpGet("/api/visitors/visits")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<VisitResponse>))]
    [EndpointDescription("List all visits matching the criteria")]
    [EndpointSummary("List visits")]
    public async Task<IResult> ListVisits(
        [FromQuery] ListVisitsRequest request,
        [FromServices] VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<Visit> query = db.Visits.Include(x => x.Invitations).AsQueryable();

        if (request.WithStatus.Count > 0)
            query = query.Where(x => request.WithStatus.Contains(x.Status));

        if (request.OrganizerId.HasValue)
            query = query.Where(x => request.OrganizerId.Value == x.OrganizerId);

        if (request.After.HasValue)
            query = query.Where(x => x.Start >= request.After.Value);

        if (request.Before.HasValue)
            query = query.Where(x => x.Stop <= request.Before.Value);

        query = query.OrderBy(x => x.Start);

        IPaged<Visit> result = await query.GetPageAsync(request.Page, request.PageSize, cancellationToken);

        Guid[] organizerIds = result.Items.Select(x => x.OrganizerId).Distinct().ToArray();
        List<Organizer> organizers = await db.Organizers
            .Where(x => organizerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        Dictionary<Guid, Organizer> organizerMap = organizers.ToDictionary(x => x.Id);

        return Results.Ok(result.Map(visit => visit.ToResponse(organizerMap[visit.OrganizerId])));
    }

    [HttpPost("/api/visitors/visits")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VisitResponse))]
    [EndpointDescription("Create a new visit")]
    [EndpointSummary("Create a new visit")]
    public async Task<IResult> CreateVisit(
        [FromBody] CreateVisitRequest request,
        [FromServices] VisitService visitService,
        CancellationToken cancellationToken = default
    )
    {
        Result<Visit, VisitErrors> result = await visitService.Create(
            request.Organizer,
            request.Summary,
            request.Start,
            request.Stop,
            request.LocationId,
            cancellationToken
        );

        return result.AsResponse(MapError);
    }

    [HttpPost("/api/visitors/visits/{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Cancel a visit")]
    [EndpointSummary("Cancel a visit")]
    public async Task<IResult> CancelVisit(
        Guid id,
        [FromServices] VisitService visitService,
        [FromServices] VisitorPreOnboardingSagaService onboardingSagaService,
        CancellationToken cancellationToken = default
    )
    {
        Result<VisitErrors> result = await visitService.Cancel(id, cancellationToken);

        if (result.IsSuccess(out _))
        {
            await onboardingSagaService.CancelForVisitAsync(id, cancellationToken);
        }

        return result.AsResponse(MapError);
    }

    [HttpPost("/api/visitors/visits/{id:guid}/reschedule")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Reschedule a visit")]
    [EndpointSummary("Reschedule a visit")]
    public async Task<IResult> RescheduleVisit(
        Guid id,
        [FromBody] RescheduleVisitRequest request,
        [FromServices] VisitService visitService,
        [FromServices] VisitorPreOnboardingSagaService onboardingSagaService,
        CancellationToken cancellationToken = default
    )
    {
        Result<VisitErrors> result = await visitService.Reschedule(
            id,
            request.Start,
            request.Stop,
            cancellationToken
        );

        if (result.IsSuccess(out _))
        {
            await onboardingSagaService.VisitRescheduled(id, request.Start, cancellationToken);
        }

        return result.AsResponse(MapError);
    }

    [HttpPut("/api/visitors/visits/{id:guid}/summary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Update visit summary")]
    [EndpointSummary("Update visit summary")]
    public async Task<IResult> UpdateVisitSummary(
        Guid id,
        [FromBody] UpdateVisitSummaryRequest request,
        [FromServices] VisitService visitService,
        CancellationToken cancellationToken = default
    )
    {
        Result<VisitErrors> result = await visitService.UpdateSummary(id, request.Summary, cancellationToken);

        return result.AsResponse(MapError);
    }

    [HttpPost("/api/visitors/visits/{id:guid}/relocate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Relocate a visit")]
    [EndpointSummary("Relocate a visit")]
    public async Task<IResult> RelocateVisit(
        Guid id,
        [FromBody] RelocateVisitRequest request,
        [FromServices] VisitService visitService,
        CancellationToken cancellationToken = default
    )
    {
        Result<VisitErrors> result = await visitService.Relocate(id, request.LocationId, cancellationToken);

        return result.AsResponse(MapError);
    }

    [HttpPost("/api/visitors/visits/{id:guid}/invitations")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VisitInvitationResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Invite someone to a visit")]
    [EndpointSummary("Invite someone to a visit")]
    public async Task<IResult> InviteToVisit(
        Guid id,
        [FromBody] InviteVisitRequest request,
        [FromServices] VisitService visitService,
        [FromServices] VisitorPreOnboardingSagaService onboardingSagaService,
        [FromServices] VisitorsDbContext db,
        CancellationToken cancellationToken = default
    )
    {
        Result<VisitInvitation, VisitErrors> result = await visitService.Invite(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Company,
            cancellationToken
        );

        if (result.IsSuccess(out VisitInvitation? invitation))
        {
            Visit visit = await db.Visits.SingleAsync(x => x.Id == id, cancellationToken);
            VisitorPreOnboardingSaga saga = await onboardingSagaService.StartAsync(
                visit.Id,
                invitation.Id,
                visit.Start,
                cancellationToken
            );
            await onboardingSagaService.ProcessAsync(saga, cancellationToken);
        }

        return result.Map(x => x.ToResponse()).AsResponse(MapError);
    }

    [HttpPost("/api/visitors/visits/{visitId:guid}/invitations/{invitationId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Confirm participation in a visit")]
    [EndpointSummary("Confirm invitation")]
    public async Task<IResult> ConfirmInvitation(

        Guid visitId,
        Guid invitationId,
        [FromBody] ConfirmInvitationRequest request,
        [FromServices] VisitService visitService,
        [FromServices] VisitorPreOnboardingSagaService onboardingSagaService,
        CancellationToken cancellationToken = default
    )
    {
        Result<Visitor, VisitErrors> result = await visitService.AcceptInvitation(
            visitId,
            invitationId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Company,
            request.Transport,
            request.LicensePlate,
            cancellationToken
        );

        if (result.IsSuccess(out Visitor visitor))
        {
            await onboardingSagaService.ConfirmAsync(visitor, visitId, invitationId, cancellationToken);
        }

        return result.AsResponse(MapError);
    }

    [HttpPost("/api/visitors/visits/{visitId:guid}/invitations/{invitationId:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Reject participation in a visit")]
    [EndpointSummary("Reject invitation")]
    public async Task<IResult> RejectInvitation(
        Guid visitId,
        Guid invitationId,
        [FromServices] VisitService visitService,
        [FromServices] VisitorPreOnboardingSagaService onboardingSagaService,
        CancellationToken cancellationToken = default
    )
    {
        Result<VisitErrors> result = await visitService.RejectInvitation(visitId, invitationId, cancellationToken);

        if (result.IsSuccess(out _))
        {
            await onboardingSagaService.RejectAsync(visitId, invitationId, cancellationToken);
        }

        return result.AsResponse(MapError);
    }

    private static (int statusCode, ProblemDetails? problemDetails) MapError(VisitErrors errors)
    {
        return errors switch
        {
            VisitErrors.VisitNotFound => Problem(StatusCodes.Status404NotFound, "Visit not found."),
            VisitErrors.OrganizerNotFound => Problem(
                StatusCodes.Status404NotFound,
                "Organizer not found."
            ),
            VisitErrors.InvitationNotFound => Problem(
                StatusCodes.Status404NotFound,
                "Invitation not found."
            ),
            VisitErrors.StartMustBeBeforeStop => Problem(
                StatusCodes.Status400BadRequest,
                "Start must be before stop."
            ),
            VisitErrors.StopMustBeFuture => Problem(
                StatusCodes.Status400BadRequest,
                "Stop must be in the future."
            ),
            VisitErrors.Cancelled => Problem(StatusCodes.Status409Conflict, "Visit is cancelled."),
            VisitErrors.Completed => Problem(StatusCodes.Status409Conflict, "Visit is completed."),
            VisitErrors.AlreadyCancelled => Problem(
                StatusCodes.Status409Conflict,
                "Visit is already cancelled."
            ),
            VisitErrors.InvalidStatus => Problem(
                StatusCodes.Status409Conflict,
                "Visit status does not allow this operation."
            ),
            VisitErrors.DuplicateInvitationEmail => Problem(
                StatusCodes.Status409Conflict,
                "Invitation email already exists."
            ),
            VisitErrors.LicensePlateRequired => Problem(
                StatusCodes.Status400BadRequest,
                "License plate is required."
            ),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected visit error."),
        };
    }

    private static (int statusCode, ProblemDetails problemDetails) Problem(
        int statusCode,
        string detail
    ) => (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
