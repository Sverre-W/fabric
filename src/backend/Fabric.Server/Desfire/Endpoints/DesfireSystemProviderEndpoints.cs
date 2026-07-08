using System.Text.Json;
using Fabric.Server.Core;
using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Endpoints;

public static class DesfireSystemProviderEndpoints
{
    public static IEndpointRouteBuilder MapDesfireSystemProviderEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder providers = app.MapGroup("/api/desfire/system-providers");
        providers.MapGet("", ListSystemProviders).Produces<Page<SystemProviderResponse>>();
        providers.MapPost("", CreateSystemProvider).Produces<SystemProviderResponse>(StatusCodes.Status201Created);
        providers.MapGet("/{id:guid}", GetSystemProvider).Produces<SystemProviderResponse>().Produces(StatusCodes.Status404NotFound);
        providers.MapDelete("/{id:guid}", DeleteSystemProvider).Produces(StatusCodes.Status204NoContent).Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static async Task<IResult> ListSystemProviders([AsParameters] BaseListRequest request, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        IPaged<DesfireSystemProvider> result = await db.SystemProviders.AsNoTracking().OrderBy(provider => provider.Name).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(provider => provider.ToResponse()));
    }

    private static async Task<IResult> CreateSystemProvider([FromBody] CreateSystemProviderRequest request, DesfireDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        IResult? validation = ValidateCreateRequest(request);
        if (validation is not null)
            return validation;

        string name = request.Name.Trim();
        bool exists = await db.SystemProviders.AnyAsync(provider => provider.Name == name, cancellationToken);
        if (exists)
            return Results.Problem("System provider name already exists.", statusCode: StatusCodes.Status409Conflict);

        DesfireSystemProvider provider = DesfireSystemProvider.Create(name, request.ProviderType, request.FixedValue, request.InitialValue, timeProvider.GetUtcNow());
        db.SystemProviders.Add(provider);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/desfire/system-providers/{provider.Id}", provider.ToResponse());
    }

    private static async Task<IResult> GetSystemProvider(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        DesfireSystemProvider? provider = await db.SystemProviders.AsNoTracking().SingleOrDefaultAsync(provider => provider.Id == id, cancellationToken);
        return provider is null ? Results.NotFound() : Results.Ok(provider.ToResponse());
    }

    private static async Task<IResult> DeleteSystemProvider(Guid id, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        DesfireSystemProvider? provider = await db.SystemProviders.SingleOrDefaultAsync(provider => provider.Id == id, cancellationToken);
        if (provider is null)
            return Results.NotFound();

        bool referenced = await IsReferencedAsync(id, db, cancellationToken);
        if (referenced)
            return Results.Problem("Cannot delete a system provider referenced by a transformation.", statusCode: StatusCodes.Status409Conflict);

        db.SystemProviders.Remove(provider);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static IResult? ValidateCreateRequest(CreateSystemProviderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.Problem("System provider name is required.", statusCode: StatusCodes.Status400BadRequest);

        return request.ProviderType switch
        {
            SystemVariableProviderKind.Fixed when string.IsNullOrWhiteSpace(request.FixedValue) => Results.Problem("Fixed system provider requires a value.", statusCode: StatusCodes.Status400BadRequest),
            SystemVariableProviderKind.Sequence when request.InitialValue is null or < 0 => Results.Problem("Sequence system provider requires an initial value greater than or equal to zero.", statusCode: StatusCodes.Status400BadRequest),
            SystemVariableProviderKind.Fixed or SystemVariableProviderKind.Sequence => null,
            _ => Results.Problem("Unsupported system provider type.", statusCode: StatusCodes.Status400BadRequest)
        };
    }

    private static async Task<bool> IsReferencedAsync(Guid id, DesfireDbContext db, CancellationToken cancellationToken)
    {
        string[] configJson = await db.Transformations.AsNoTracking().Select(transformation => transformation.VariableConfigsJson).ToArrayAsync(cancellationToken);
        return configJson.Any(json => (JsonSerializer.Deserialize<TransformationVariableConfigRequest[]>(json, DesfireJson.Options) ?? []).Any(config => config.SystemProviderId == id));
    }
}
