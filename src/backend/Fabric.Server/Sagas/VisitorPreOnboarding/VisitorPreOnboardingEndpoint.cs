using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

[ApiController]
public class VisitorPreOnboardingSagaController
{
    [HttpPost("/api/sagas/visitor-pre-onboarding/{id:guid}/retry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [EndpointDescription("Retry an expired visitor pre-onboarding saga")]
    [EndpointSummary("Retry saga")]
    public async Task<IResult> RetrySaga(
        Guid id,
        [FromServices] VisitorPreOnboardingSagaService service,
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

    [HttpGet("/api/sagas/visitor-pre-onboarding/{visitId:guid}")]
    [ProducesResponseType(typeof(List<VisitorPreOnboardingSaga>), StatusCodes.Status200OK)]
    public async Task<IResult> GetOnboardingSagas(
        Guid visitId,
        [FromServices] SagasDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        List<VisitorPreOnboardingSaga> sagas = await dbContext
            .VisitorPreOnboardingSagas.AsNoTracking()
            .Where(x => x.VisitId == visitId)
            .ToListAsync(cancellationToken);

        return Results.Ok(sagas);
    }

    [HttpGet("/api/sagas/visitor-pre-onboarding/{visitId:guid}/{invitationId:guid}")]
    [ProducesResponseType(typeof(VisitorPreOnboardingSaga), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetOnboardingSaga(
        Guid visitId,
        Guid invitationId,
        [FromServices] SagasDbContext dbContext,
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

