using Fabric.Hardware.Desfire.Protocol;
using Fabric.Server.Core;

namespace Fabric.Server.Desfire.Domain;

public sealed class KeyGroup
{
    private KeyGroup() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public KeyType KeyType { get; private set; }
    public bool Locked { get; private set; }
    public Guid? DiversificationStrategyId { get; private set; }
    public List<KeyGroupKeySet> KeySets { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<KeyGroup, KeyGroupError> Create(string name, KeyType keyType, Guid? diversificationStrategyId, IEnumerable<KeyGroupKeySetInput> keySets, DateTimeOffset now)
    {
        var keySetList = keySets.Select(KeyGroupKeySet.Create).ToList();
        if (keySetList.Count == 0)
            return Result.Failure<KeyGroup, KeyGroupError>(KeyGroupError.EmptyKeySets);

        if (diversificationStrategyId is null && keySetList.SelectMany(keySet => keySet.Keys).Any(key => key.IsDiversified))
            return Result.Failure<KeyGroup, KeyGroupError>(KeyGroupError.DiversifiedKeyRequiresStrategy);

        return Result.Success<KeyGroup, KeyGroupError>(new KeyGroup
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            KeyType = keyType,
            DiversificationStrategyId = diversificationStrategyId,
            KeySets = keySetList,
            Locked = false,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    public Result<KeyGroupError> Update(string name, Guid? diversificationStrategyId, IEnumerable<KeyGroupKeySetInput> keySets, DateTimeOffset now)
    {
        if (Locked)
            return Result.Failure<KeyGroupError>(KeyGroupError.CannotEditLocked);

        var keySetList = keySets.Select(KeyGroupKeySet.Create).ToList();
        if (keySetList.Count == 0)
            return Result.Failure<KeyGroupError>(KeyGroupError.EmptyKeySets);

        if (diversificationStrategyId is null && keySetList.SelectMany(keySet => keySet.Keys).Any(key => key.IsDiversified))
            return Result.Failure<KeyGroupError>(KeyGroupError.DiversifiedKeyRequiresStrategy);

        if (!MatchesExistingStructure(keySetList))
            return Result.Failure<KeyGroupError>(KeyGroupError.CannotChangeKeyStructure);

        Name = name.Trim();
        DiversificationStrategyId = diversificationStrategyId;
        UpdateKeys(keySetList);
        UpdatedAt = now;
        return Result.Success<KeyGroupError>();
    }

    private bool MatchesExistingStructure(IReadOnlyCollection<KeyGroupKeySet> keySets)
    {
        if (KeySets.Count != keySets.Count)
            return false;

        if (keySets.Select(keySet => keySet.KeySetId).Distinct().Count() != keySets.Count)
            return false;

        foreach (KeyGroupKeySet keySet in KeySets)
        {
            KeyGroupKeySet? updatedKeySet = keySets.SingleOrDefault(candidate => candidate.KeySetId == keySet.KeySetId);
            if (updatedKeySet is null || updatedKeySet.Keys.Count != keySet.Keys.Count)
                return false;

            if (updatedKeySet.Keys.Select(key => key.KeyId).Distinct().Count() != updatedKeySet.Keys.Count)
                return false;

            if (keySet.Keys.Any(key => updatedKeySet.Keys.All(candidate => candidate.KeyId != key.KeyId)))
                return false;
        }

        return true;
    }

    private void UpdateKeys(IReadOnlyCollection<KeyGroupKeySet> keySets)
    {
        foreach (KeyGroupKeySet keySet in KeySets)
        {
            KeyGroupKeySet updatedKeySet = keySets.Single(candidate => candidate.KeySetId == keySet.KeySetId);
            foreach (KeyGroupKey key in keySet.Keys)
            {
                KeyGroupKey updatedKey = updatedKeySet.Keys.Single(candidate => candidate.KeyId == key.KeyId);
                key.Update(updatedKey.ProtectedValue, updatedKey.IsDiversified);
            }
        }
    }

    public Result<KeyGroupError> Lock(DateTimeOffset now)
    {
        if (Locked)
            return Result.Failure<KeyGroupError>(KeyGroupError.AlreadyLocked);

        Locked = true;
        UpdatedAt = now;
        return Result.Success<KeyGroupError>();
    }
}

public sealed class KeyGroupKeySet
{
    private KeyGroupKeySet() { }

    public Guid Id { get; private set; }
    public int KeySetId { get; private set; }
    public List<KeyGroupKey> Keys { get; private set; } = [];

    internal static KeyGroupKeySet Create(KeyGroupKeySetInput input) => new()
    {
        Id = Guid.NewGuid(),
        KeySetId = input.KeySetId,
        Keys = input.Keys.Select(KeyGroupKey.Create).ToList()
    };
}

public sealed class KeyGroupKey
{
    private KeyGroupKey() { }

    public Guid Id { get; private set; }
    public int KeyId { get; private set; }
    public string ProtectedValue { get; private set; } = default!;
    public bool IsDiversified { get; private set; }

    internal static KeyGroupKey Create(KeyGroupKeyInput input) => new()
    {
        Id = Guid.NewGuid(),
        KeyId = input.KeyId,
        ProtectedValue = input.ProtectedValue,
        IsDiversified = input.IsDiversified
    };

    internal void Update(string protectedValue, bool isDiversified)
    {
        ProtectedValue = protectedValue;
        IsDiversified = isDiversified;
    }
}

public sealed record KeyGroupKeySetInput(int KeySetId, IReadOnlyList<KeyGroupKeyInput> Keys);

public sealed record KeyGroupKeyInput(int KeyId, string ProtectedValue, bool IsDiversified);
