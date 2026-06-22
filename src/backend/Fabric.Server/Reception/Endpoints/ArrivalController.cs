using Fabric.Server.Core;
using Fabric.Server.Reception.Application;
using Fabric.Server.Reception.Contracts;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Reception.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Reception.Endpoints;

public static class ArrivalEndpoints
{
    public static IEndpointRouteBuilder MapReceptionEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder arrivals = app.MapGroup("/api/reception/arrivals");

        arrivals.MapGet("/{id:guid}", GetArrivalById)
            .WithDescription("Retrieve an arrival by id")
            .WithSummary("Retrieve an arrival by id")
            .Produces<ArrivalResponse>()
            .Produces(StatusCodes.Status404NotFound);
        arrivals.MapGet("", ListArrivals)
            .WithDescription("List all arrivals matching the criteria")
            .WithSummary("List arrivals")
            .Produces<Page<ArrivalResponse>>();
        arrivals.MapPost("/{id:guid}/onboard", OnboardArrival)
            .WithDescription("Onboard an arrival with documents")
            .WithSummary("Onboard arrival")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        arrivals.MapPost("/{id:guid}/offboard", OffboardArrival)
            .WithDescription("Offboard an arrival")
            .WithSummary("Offboard arrival")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        arrivals.MapPost("/{id:guid}/check-in", CheckInArrival)
            .WithDescription("Check in an arrival")
            .WithSummary("Check in arrival")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        arrivals.MapPost("/{id:guid}/check-out", CheckOutArrival)
            .WithDescription("Check out an arrival")
            .WithSummary("Check out arrival")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> GetArrivalById(
        Guid id,
        ReceptionDbContext db,
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

    private static async Task<IResult> ListArrivals(
        [AsParameters] ListArrivalsRequest request,
        [FromQuery] DateTimeOffset? expectedArrivalAfter,
        [FromQuery] DateTimeOffset? expectedArrivalBefore,
        [FromQuery] DateTimeOffset? onboardedBefore,
        [FromQuery] DateTimeOffset? offboardedAfter,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        ReceptionDbContext db,
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

        if (expectedArrivalAfter.HasValue)
            query = query.Where(a => a.ExpectedArrivalTime >= expectedArrivalAfter.Value);

        if (expectedArrivalBefore.HasValue)
            query = query.Where(a => a.ExpectedArrivalTime < expectedArrivalBefore.Value);

        if (onboardedBefore.HasValue)
            query = query.Where(a => a.OnboardedAt.HasValue && a.OnboardedAt.Value < onboardedBefore.Value);

        if (offboardedAfter.HasValue)
            query = query.Where(a => a.OffboardedAt.HasValue && a.OffboardedAt.Value >= offboardedAfter.Value);

        query = request.Status == OnboardingStatus.Offboarded
            ? query.OrderByDescending(a => a.OffboardedAt)
            : query.OrderByDescending(a => a.ExpectedArrivalTime);

        IPaged<ExpectedArrival> result = await query.GetPageAsync(page ?? 0, pageSize ?? 25, cancellationToken);
        return Results.Ok(result.Map(a => a.ToResponse()));
    }

    private static async Task<IResult> OnboardArrival(
        Guid id,
        [FromBody] OnboardArrivalRequest request,
        ReceptionService receptionService,
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

    private static async Task<IResult> OffboardArrival(
        Guid id,
        ReceptionService receptionService,
        CancellationToken cancellationToken = default)
    {
        Result<ReceptionErrors> result = await receptionService.Offboard(id, cancellationToken);
        return result.AsResponse(MapError);
    }

    private static async Task<IResult> CheckInArrival(
        Guid id,
        ReceptionService receptionService,
        CancellationToken cancellationToken = default)
    {
        Result<ReceptionErrors> result = await receptionService.CheckIn(id, cancellationToken);
        return result.AsResponse(MapError);
    }

    private static async Task<IResult> CheckOutArrival(
        Guid id,
        ReceptionService receptionService,
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
            ReceptionErrors.ExpectedArrivalMustBeBeforeExpectedOffboard => Problem(StatusCodes.Status400BadRequest, "Expected arrival must be before expected offboard."),
            _ => Problem(StatusCodes.Status500InternalServerError, "Unexpected reception error."),
        };
    }

    private static (int statusCode, ProblemDetails problemDetails) Problem(int statusCode, string detail) => (statusCode, new ProblemDetails { Status = statusCode, Detail = detail });
}
