using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public static class VisitorPreOnboardingSagaEndpoints
{
    public static IEndpointRouteBuilder MapVisitorPreOnboardingSagaEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/sagas/visitor-pre-onboarding");

        group.MapPost("/{id:guid}/retry", RetrySaga)
            .WithDescription("Retry an expired visitor pre-onboarding saga")
            .WithSummary("Retry saga")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);
        group.MapGet("/{visitId:guid}", GetOnboardingSagas)
            .Produces<List<VisitorPreOnboardingSaga>>();
        group.MapGet("/{visitId:guid}/{invitationId:guid}", GetOnboardingSaga)
            .Produces<VisitorPreOnboardingSaga>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RetrySaga(
        Guid id,
        VisitorPreOnboardingSagaService service,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await service.RetryAsync(id, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(
                new ProblemDetails { Status = StatusCodes.Status409Conflict, Detail = ex.Message }
            );
        }
    }

    private static async Task<IResult> GetOnboardingSagas(
        Guid visitId,
        SagasDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        List<VisitorPreOnboardingSaga> sagas = await dbContext
            .VisitorPreOnboardingSagas.AsNoTracking()
            .Where(x => x.VisitId == visitId)
            .ToListAsync(cancellationToken);

        return Results.Ok(sagas);
    }

    private static async Task<IResult> GetOnboardingSaga(
        Guid visitId,
        Guid invitationId,
        SagasDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        VisitorPreOnboardingSaga? saga = await dbContext
        .VisitorPreOnboardingSagas.AsNoTracking()
        .FirstOrDefaultAsync(
            x => x.InvitationId == invitationId && x.VisitId == visitId,
            cancellationToken: cancellationToken
        );

        if (saga is null)
            return Results.NotFound();

        return Results.Ok(saga);
    }
}
