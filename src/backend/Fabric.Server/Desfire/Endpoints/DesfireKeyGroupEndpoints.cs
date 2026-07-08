using System.Security.Cryptography;
using Fabric.Server.Core;
using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KeyType = Fabric.Hardware.Desfire.Protocol.KeyType;

namespace Fabric.Server.Desfire.Endpoints;

public static class DesfireKeyGroupEndpoints
{
    private const int MaxGeneratedKeySets = 16;
    private const int MaxGeneratedKeys = 16;

    public static IEndpointRouteBuilder MapDesfireKeyGroupEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder groups = app.MapGroup("/api/desfire/key-groups");
        groups.MapGet("", ListKeyGroups).Produces<Page<KeyGroupResponse>>();
        groups.MapPost("", CreateKeyGroup).Produces<KeyGroupResponse>(StatusCodes.Status201Created);
        groups.MapGet("/{id:guid}", GetKeyGroup).Produces<KeyGroupResponse>().Produces(StatusCodes.Status404NotFound);
        groups.MapPut("/{id:guid}", UpdateKeyGroup).Produces<KeyGroupResponse>().Produces(StatusCodes.Status404NotFound);
        groups.MapPost("/{id:guid}/lock", LockKeyGroup).Produces<KeyGroupResponse>().Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static async Task<IResult> ListKeyGroups([AsParameters] BaseListRequest request, DesfireDbContext db, CancellationToken cancellationToken = default)
    {
        IPaged<KeyGroup> result = await db.KeyGroups.AsNoTracking().Include(group => group.KeySets).ThenInclude(keySet => keySet.Keys).OrderBy(group => group.Name).GetPageAsync(request.Page, request.PageSize, cancellationToken);
        return Results.Ok(result.Map(group => group.ToResponse()));
    }

    private static async Task<IResult> CreateKeyGroup([FromBody] CreateKeyGroupRequest request, DesfireDbContext db, IDesfireKeyProtector keyProtector, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        if (request.KeyType == KeyType.None)
            return Results.Problem("Key type is required.", statusCode: StatusCodes.Status422UnprocessableEntity);

        if (request.NumberOfKeySets is < 1 or > MaxGeneratedKeySets)
            return Results.Problem($"Number of key sets must be between 1 and {MaxGeneratedKeySets}.", statusCode: StatusCodes.Status422UnprocessableEntity);

        if (request.NumberOfKeys is < 1 or > MaxGeneratedKeys)
            return Results.Problem($"Number of keys must be between 1 and {MaxGeneratedKeys}.", statusCode: StatusCodes.Status422UnprocessableEntity);

        Result<KeyGroup, KeyGroupError> result = KeyGroup.Create(request.Name, request.KeyType, null, GenerateRandomKeys(request, keyProtector), timeProvider.GetUtcNow());
        if (result.IsFailure(out KeyGroupError error))
            return MapError(error);

        result.IsSuccess(out KeyGroup group);
        db.KeyGroups.Add(group);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/desfire/key-groups/{group.Id}", group.ToResponse(keyProtector));
    }

    private static async Task<IResult> GetKeyGroup(Guid id, DesfireDbContext db, IDesfireKeyProtector keyProtector, CancellationToken cancellationToken = default)
    {
        KeyGroup? group = await db.KeyGroups.AsNoTracking().Include(group => group.KeySets).ThenInclude(keySet => keySet.Keys).SingleOrDefaultAsync(group => group.Id == id, cancellationToken);
        return group is null ? Results.NotFound() : Results.Ok(group.ToResponse(keyProtector));
    }

    private static async Task<IResult> UpdateKeyGroup(Guid id, [FromBody] UpdateKeyGroupRequest request, DesfireDbContext db, IDesfireKeyProtector keyProtector, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        KeyGroup? group = await db.KeyGroups.Include(group => group.KeySets).ThenInclude(keySet => keySet.Keys).SingleOrDefaultAsync(group => group.Id == id, cancellationToken);
        if (group is null)
            return Results.NotFound();

        if (request.DiversificationStrategyId is not null && !await db.KeyDiversificationStrategies.AnyAsync(strategy => strategy.Id == request.DiversificationStrategyId, cancellationToken))
            return Results.Problem("Key diversification strategy does not exist.", statusCode: StatusCodes.Status409Conflict);

        Result<KeyGroupError> result = group.Update(request.Name, request.DiversificationStrategyId, Protect(request.KeySets, keyProtector), timeProvider.GetUtcNow());
        if (result.IsFailure(out KeyGroupError error))
            return MapError(error);

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(group.ToResponse(keyProtector));
    }

    private static async Task<IResult> LockKeyGroup(Guid id, DesfireDbContext db, TimeProvider timeProvider, CancellationToken cancellationToken = default)
    {
        KeyGroup? group = await db.KeyGroups.Include(group => group.KeySets).ThenInclude(keySet => keySet.Keys).SingleOrDefaultAsync(group => group.Id == id, cancellationToken);
        if (group is null)
            return Results.NotFound();

        Result<KeyGroupError> result = group.Lock(timeProvider.GetUtcNow());
        if (result.IsFailure(out KeyGroupError error))
            return MapError(error);

        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(group.ToResponse());
    }

    private static KeyGroupKeySetInput[] Protect(IReadOnlyList<KeyGroupKeySetRequest> keySets, IDesfireKeyProtector keyProtector) =>
        [.. keySets.Select(keySet => new KeyGroupKeySetInput(keySet.KeySetId, [.. keySet.Keys.Select(key => new KeyGroupKeyInput(key.KeyId, keyProtector.Protect(key.Value), key.IsDiversified))]))];

    private static KeyGroupKeySetInput[] GenerateRandomKeys(CreateKeyGroupRequest request, IDesfireKeyProtector keyProtector)
    {
        int keyLength = GetKeyLengthBytes(request.KeyType);
        return [.. Enumerable.Range(0, request.NumberOfKeySets).Select(keySetId => new KeyGroupKeySetInput(
            keySetId,
            [.. Enumerable.Range(0, request.NumberOfKeys).Select(keyId => new KeyGroupKeyInput(
                keyId,
                keyProtector.Protect(Convert.ToHexString(RandomNumberGenerator.GetBytes(keyLength))),
                false))]))];
    }

    private static int GetKeyLengthBytes(KeyType keyType) => keyType switch
    {
        KeyType.Aes => 16,
        KeyType.TDes => 8,
        KeyType.Tdes2K => 16,
        KeyType.Tdes3K => 24,
        _ => throw new ArgumentOutOfRangeException(nameof(keyType), keyType, "Unsupported key type.")
    };

    private static IResult MapError(KeyGroupError error) => error switch
    {
        KeyGroupError.AlreadyLocked => Results.Problem("Key group is already locked.", statusCode: StatusCodes.Status409Conflict),
        KeyGroupError.CannotEditLocked => Results.Problem("Cannot edit a locked key group.", statusCode: StatusCodes.Status422UnprocessableEntity),
        KeyGroupError.CannotChangeKeyStructure => Results.Problem("Key group key set and key IDs cannot be changed after generation.", statusCode: StatusCodes.Status422UnprocessableEntity),
        KeyGroupError.DiversifiedKeyRequiresStrategy => Results.Problem("Diversified keys require a key diversification strategy.", statusCode: StatusCodes.Status422UnprocessableEntity),
        KeyGroupError.EmptyKeySets => Results.Problem("Key group must contain at least one key set.", statusCode: StatusCodes.Status422UnprocessableEntity),
        _ => Results.Problem("Unexpected key group error.", statusCode: StatusCodes.Status500InternalServerError)
    };
}
