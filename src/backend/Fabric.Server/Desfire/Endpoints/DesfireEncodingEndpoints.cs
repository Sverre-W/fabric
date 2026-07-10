using System.Text.Json;
using Fabric.Server.Core;
using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Endpoints;

public static class DesfireEncodingEndpoints
{
    public static IEndpointRouteBuilder MapDesfireEncodingEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder batches = app.MapGroup("/api/desfire/encoding-batches");
        batches.MapGet("", ListBatches).Produces<Page<EncodingBatchResponse>>();
        batches.MapPost("", CreateBatch).Produces<EncodingBatchResponse>(StatusCodes.Status201Created);
        batches.MapGet("/{id:guid}", GetBatch).Produces<EncodingBatchResponse>().Produces(StatusCodes.Status404NotFound);

        RouteGroupBuilder runs = app.MapGroup("/api/desfire/encoding-runs");
        runs.MapGet("", ListRuns).Produces<Page<EncodingRunResponse>>();
        runs.MapGet("/{id:guid}", GetRun).Produces<EncodingRunResponse>().Produces(StatusCodes.Status404NotFound);

        RouteGroupBuilder adHoc = app.MapGroup("/api/desfire/ad-hoc-encodings");
        adHoc.MapPost("", CreateAdHocEncoding).Produces<EncodingRunResponse>().Produces(StatusCodes.Status404NotFound).Produces(StatusCodes.Status409Conflict);
        return app;
    }

    private static async Task<IResult> ListBatches([AsParameters] BaseListRequest request, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        IPaged<EncodingBatch> result = await db.EncodingBatches.AsNoTracking().OrderByDescending(batch => batch.CreatedAt).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        Dictionary<Guid, EncodingBatchRunSummary> summaries = await GetBatchRunSummariesAsync(db, result.Items.Select(batch => batch.Id).ToArray(), cancellationToken);
        return Results.Ok(result.Map(batch => batch.ToResponse(summaries.GetValueOrDefault(batch.Id))));
    }

    private static async Task<IResult> CreateBatch([FromBody] CreateEncodingBatchRequest request, DesfireDbContext db, DesfireEncodingWakeChannel wakeChannel, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.Problem("Print batch name is required.", statusCode: StatusCodes.Status400BadRequest);

        Transformation? transformation = await db.Transformations.AsNoTracking().SingleOrDefaultAsync(transformation => transformation.Id == request.TransformationId, cancellationToken);
        if (transformation is null)
            return Results.Problem("Transformation does not exist.", statusCode: StatusCodes.Status409Conflict);

        DesfireEncoder? encoder = await db.Encoders.AsNoTracking().SingleOrDefaultAsync(encoder => encoder.Id == request.EncoderId, cancellationToken);
        if (encoder is null)
            return Results.Problem("Encoder does not exist.", statusCode: StatusCodes.Status409Conflict);

        if (!encoder.Enabled)
            return Results.Problem("Encoder is disabled.", statusCode: StatusCodes.Status409Conflict);

        DateTimeOffset now = timeProvider.GetUtcNow();
        EncodingBatch batch = EncodingBatch.Create(
            request.Name,
            encoder.Id,
            request.TransformationId,
            JsonSerializer.Serialize(request.OriginalInput, DesfireJson.Options),
            JsonSerializer.Serialize(request.NormalizedRows, DesfireJson.Options),
            now);

        db.EncodingBatches.Add(batch);
        if (request.NormalizedRows.ValueKind != JsonValueKind.Array)
            return Results.Problem("Normalized rows must be a JSON array.", statusCode: StatusCodes.Status400BadRequest);

        foreach (JsonElement row in request.NormalizedRows.EnumerateArray())
        {
            db.EncodingRuns.Add(EncodingRun.Create(
                request.TransformationId,
                batch.Id,
                encoder.Id,
                null,
                EncodingRunKind.Batch,
                DesfireEncodingSources.PrintBatch,
                JsonSerializer.Serialize(row, DesfireJson.Options),
                transformation.VariableConfigsJson,
                encoder.AgentId,
                encoder.DeviceId,
                request.Priority,
                now));
        }

        await db.SaveChangesAsync(cancellationToken);
        wakeChannel.Signal();
        return Results.Created($"/api/desfire/encoding-batches/{batch.Id}", batch.ToResponse());
    }

    private static async Task<IResult> GetBatch(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        EncodingBatch? batch = await db.EncodingBatches.AsNoTracking().SingleOrDefaultAsync(batch => batch.Id == id, cancellationToken);
        if (batch is null)
            return Results.NotFound();

        Dictionary<Guid, EncodingBatchRunSummary> summaries = await GetBatchRunSummariesAsync(db, [batch.Id], cancellationToken);
        return Results.Ok(batch.ToResponse(summaries.GetValueOrDefault(batch.Id)));
    }

    private static async Task<IResult> ListRuns([AsParameters] BaseListRequest request, [FromQuery] Guid? transformationId, [FromQuery] Guid? batchId, [FromQuery] string? cardUid, [FromQuery] string? source, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        IQueryable<EncodingRun> query = db.EncodingRuns.AsNoTracking();
        if (transformationId is not null)
            query = query.Where(run => run.TransformationId == transformationId);

        if (batchId is not null)
            query = query.Where(run => run.BatchId == batchId);

        if (!string.IsNullOrWhiteSpace(cardUid))
            query = query.Where(run => run.CardUid == cardUid);

        if (!string.IsNullOrWhiteSpace(source))
        {
            string normalizedSource = source.Trim().ToLowerInvariant();
            query = query.Where(run => run.Source == normalizedSource);
        }

        IPaged<EncodingRun> result = await query.OrderByDescending(run => run.RequestedAt).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(run => run.ToResponse()));
    }

    private static async Task<IResult> GetRun(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        EncodingRun? run = await db.EncodingRuns.AsNoTracking().SingleOrDefaultAsync(run => run.Id == id, cancellationToken);
        return run is null ? Results.NotFound() : Results.Ok(run.ToResponse());
    }

    private static async Task<IResult> CreateAdHocEncoding([FromBody] CreateAdHocEncodingRequest request, DesfireEncodingService encodingService, CancellationToken cancellationToken = default)
    {
        DesfireEncodingResult result = await encodingService.CreateAdHocAsync(request, cancellationToken);
        if (result.Failure is not null)
            return result.Run is null ? result.Failure : Results.Json(result.Run.ToResponse(), statusCode: GetFailureStatusCode(result.Failure));

        return Results.Ok(result.Run!.ToResponse());
    }

    private static int GetFailureStatusCode(IResult failure) => failure switch
    {
        IStatusCodeHttpResult statusCodeResult when statusCodeResult.StatusCode is not null => statusCodeResult.StatusCode.Value,
        _ => StatusCodes.Status409Conflict
    };

    private static async Task<Dictionary<Guid, EncodingBatchRunSummary>> GetBatchRunSummariesAsync(DesfireDbContext db, IReadOnlyCollection<Guid> batchIds, CancellationToken cancellationToken)
    {
        if (batchIds.Count == 0)
            return [];

        return await db.EncodingRuns
            .AsNoTracking()
            .Where(run => run.BatchId != null && batchIds.Contains(run.BatchId.Value))
            .GroupBy(run => run.BatchId!.Value)
            .Select(group => new EncodingBatchRunSummary(
                group.Key,
                group.Count(),
                group.Count(run => run.Status == EncodingRunStatus.Pending || run.Status == EncodingRunStatus.Claimed),
                group.Count(run => run.Status == EncodingRunStatus.Running),
                group.Count(run => run.Status == EncodingRunStatus.Succeeded),
                group.Count(run => run.Status == EncodingRunStatus.Failed || run.Status == EncodingRunStatus.Timeout || run.Status == EncodingRunStatus.DeviceUnavailable),
                group.Count(run => run.Status == EncodingRunStatus.Cancelled)))
            .ToDictionaryAsync(summary => summary.BatchId, cancellationToken);
    }
}
