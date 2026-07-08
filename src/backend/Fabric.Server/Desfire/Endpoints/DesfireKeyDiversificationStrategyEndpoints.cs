using System.Text.Json;
using Fabric.Server.Core;
using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Endpoints;

public static class DesfireKeyDiversificationStrategyEndpoints
{
    public static IEndpointRouteBuilder MapDesfireKeyDiversificationStrategyEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder strategies = app.MapGroup("/api/desfire/key-diversification-strategies");
        strategies.MapGet("", ListStrategies).Produces<Page<KeyDiversificationStrategyResponse>>();
        strategies.MapPost("", CreateStrategy).Produces<KeyDiversificationStrategyResponse>(StatusCodes.Status201Created);
        strategies.MapGet("/{id:guid}", GetStrategy).Produces<KeyDiversificationStrategyResponse>().Produces(StatusCodes.Status404NotFound);
        strategies.MapPut("/{id:guid}", UpdateStrategy).Produces<KeyDiversificationStrategyResponse>().Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static async Task<IResult> ListStrategies([AsParameters] BaseListRequest request, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        IPaged<KeyDiversificationStrategyEntity> result = await db.KeyDiversificationStrategies.AsNoTracking().OrderBy(strategy => strategy.Name).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(strategy => strategy.ToResponse()));
    }

    private static async Task<IResult> CreateStrategy([FromBody] CreateKeyDiversificationStrategyRequest request, DesfireDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        string inputsJson = JsonSerializer.Serialize(request.Inputs, DesfireJson.Options);
        KeyDiversificationStrategyEntity strategy = KeyDiversificationStrategyEntity.Create(request.Name, request.Algorithm, inputsJson, timeProvider.GetUtcNow());
        db.KeyDiversificationStrategies.Add(strategy);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/desfire/key-diversification-strategies/{strategy.Id}", strategy.ToResponse());
    }

    private static async Task<IResult> GetStrategy(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        KeyDiversificationStrategyEntity? strategy = await db.KeyDiversificationStrategies.AsNoTracking().SingleOrDefaultAsync(strategy => strategy.Id == id, cancellationToken);
        return strategy is null ? Results.NotFound() : Results.Ok(strategy.ToResponse());
    }

    private static async Task<IResult> UpdateStrategy(Guid id, [FromBody] UpdateKeyDiversificationStrategyRequest request, DesfireDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        KeyDiversificationStrategyEntity? strategy = await db.KeyDiversificationStrategies.SingleOrDefaultAsync(strategy => strategy.Id == id, cancellationToken);
        if (strategy is null)
            return Results.NotFound();

        strategy.Update(request.Name, request.Algorithm, JsonSerializer.Serialize(request.Inputs, DesfireJson.Options), timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(strategy.ToResponse());
    }
}
