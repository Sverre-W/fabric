using System.Text.Json;
using Fabric.Hardware.Desfire.Encoding.Models;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Server.Desfire.Persistence;
using Microsoft.EntityFrameworkCore;
using HardwareKey = Fabric.Hardware.Desfire.Encoding.Models.Key;
using HardwareKeySet = Fabric.Hardware.Desfire.Encoding.Models.KeySet;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireKeyGroupResolver(DesfireDbContext db, IDesfireKeyProtector keyProtector) : IKeyGroupResolver
{
    public async Task<KeyGroupData?> ResolveKeyGroup(string keyGroupName, CancellationToken ct = default)
    {
        Domain.KeyGroup? group = await db.KeyGroups
            .AsNoTracking()
            .Include(keyGroup => keyGroup.KeySets)
            .ThenInclude(keySet => keySet.Keys)
            .SingleOrDefaultAsync(keyGroup => keyGroup.Name == keyGroupName, ct);

        if (group is null)
            return null;

        KeyDiversificationStrategy? strategy = null;
        if (group.DiversificationStrategyId is not null)
        {
            Domain.KeyDiversificationStrategyEntity? entity = await db.KeyDiversificationStrategies
                .AsNoTracking()
                .SingleOrDefaultAsync(candidate => candidate.Id == group.DiversificationStrategyId, ct);

            if (entity is not null)
            {
                strategy = new KeyDiversificationStrategy
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Algorithm = entity.Algorithm,
                    Inputs = JsonSerializer.Deserialize<List<DiversificationInput>>(entity.InputsJson, DesfireJson.Options) ?? []
                };
            }
        }

        return new KeyGroupData
        {
            KeyType = group.KeyType,
            KeyDiversificationStrategy = strategy,
            KeySets = [.. group.KeySets.OrderBy(keySet => keySet.KeySetId).Select(keySet => new HardwareKeySet
            {
                Id = keySet.KeySetId,
                Keys = [.. keySet.Keys.OrderBy(key => key.KeyId).Select(key => new HardwareKey
                {
                    KeyId = key.KeyId,
                    Value = keyProtector.Unprotect(key.ProtectedValue),
                    IsKeyDiversified = key.IsDiversified
                })]
            })]
        };
    }
}
