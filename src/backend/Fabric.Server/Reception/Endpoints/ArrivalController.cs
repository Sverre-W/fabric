using Fabric.Server.Core;
using Fabric.Server.Reception.Application;
using Fabric.Server.Reception.Contracts;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Endpoints;

[ApiController]
public class ArrivalController
{
    [HttpGet("/api/arrivals/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ArrivalResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointDescription("Retrieve an arrival by id")]
    [EndpointSummary("Retrieve an arrival by id")]
    public async Task<IResult> GetArrivalById(
        Guid id,
        [FromServices] ReceptionDbContext db,
        CancellationToken cancellationToken = default)
    {
        ExpectedArrival? arrival = await db.Arrivals
            .Include(a => a.Entries)
            .Include(a => a.Documents)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (arrival is null)
            return Results.NotFound();

        return Results.Ok(arrival.ToResponse());
    }

    [HttpGet("/api/arrivals")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IPaged<ArrivalResponse>))]
    [EndpointDescription("List all arrivals matching the criteria")]
    [EndpointSummary("List arrivals")]
    public async Task<IResult> ListArrivals(
        [FromQuery] ListArrivalsRequest request,
        [FromServices] ReceptionDbContext db,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ExpectedArrival> query = db.Arrivals.Include(a => a.Entries).Include(a => a.Documents).AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(a => a.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        if (request.CheckedIn.HasValue)
            query = query.Where(a => a.CheckedIn == request.CheckedIn.Value);

        if (request.LocationId.HasValue)
            query = query.Where(a => a.LocationId == request.LocationId.Value);

        query = query.OrderByDescending(a => a.ExpectedArrivalTime);

        IPaged<ExpectedArrival> result = await query.GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(a => a.ToResponse()));
    }

    [HttpPost("/api/arrivals/{id:guid}/onboard")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Onboard an arrival with documents")]
    [EndpointSummary("Onboard arrival")]
    public async Task<IResult> OnboardArrival(
        Guid id,
        [FromBody] OnboardArrivalRequest request,
        [FromServices] ReceptionService receptionService,
        CancellationToken cancellationToken = default)
    {
        List<CheckInDocumentRequirement> requiredDocs = request.RequiredDocuments
            .ConvertAll(d => new CheckInDocumentRequirement
            {
                Name = d.Name,
                Required = d.Required,
                DocumentType = d.DocumentType
            })
;

        List<CheckInDocument> providedDocs = request.ProvidedDocuments
            .ConvertAll(d => CheckInDocument.Create(d.Name, d.DocumentType, d.Content))
;

        Result<ReceptionErrors> result = await receptionService.Onboard(id, requiredDocs, providedDocs, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpPost("/api/arrivals/{id:guid}/offboard")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Offboard an arrival")]
    [EndpointSummary("Offboard arrival")]
    public async Task<IResult> OffboardArrival(
        Guid id,
        [FromServices] ReceptionService receptionService,
        CancellationToken cancellationToken = default)
    {
        Result<ReceptionErrors> result = await receptionService.Offboard(id, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpPost("/api/arrivals/{id:guid}/check-in")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Check in an arrival")]
    [EndpointSummary("Check in arrival")]
    public async Task<IResult> CheckInArrival(
        Guid id,
        [FromServices] ReceptionService receptionService,
        CancellationToken cancellationToken = default)
    {
        Result<ReceptionErrors> result = await receptionService.CheckIn(id, cancellationToken);
        return result.AsResponse(MapError);
    }

    [HttpPost("/api/arrivals/{id:guid}/check-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Check out an arrival")]
    [EndpointSummary("Check out arrival")]
    public async Task<IResult> CheckOutArrival(
        Guid id,
        [FromServices] ReceptionService receptionService,
        CancellationToken cancellationToken = default)
    {
        Result<ReceptionErrors> result = await receptionService.CheckOut(id, cancellationToken);
        return result.AsResponse(MapError);
    }

    private static (int statusCode, ProblemDetails problemDetails) MapError(ReceptionErrors errors)
    {
        return errors switch
        {
            ReceptionErrors.AlreadyOnboarded => Problem(StatusCodes.Status409Conflict, "Arrival is already onboarded."),
            ReceptionErrors.ArrivalNotFound => Problem(StatusCodes.Status404NotFound, "Arrival not found."),
            ReceptionErrors.NotYetOnboarded => Problem(StatusCodes.Status409Conflict, "Arrival is not yet onboarded."),
            ReceptionErrors.AlreadyOffboarded => Problem(StatusCodes.Status409Conflict, "Arrival is already offboarded."),
            ReceptionErrors.InvalidStatus => Problem(StatusCodes.Status409Conflict, "Arrival status does not allow this operation."),
            ReceptionErrors.MissingRequiredDocuments => Problem(StatusCodes.Status400BadRequest, "Missing required documents."),
            ReceptionErrors.NotAVisitor => Problem(StatusCodes.Status409Conflict, "Arrival is not a visitor."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected reception error."),
        };
    }

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) => (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
