namespace Fabric.Server.Desfire.Domain;

public sealed class Transformation
{
    private Transformation() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? FromChipDesignName { get; private set; }
    public bool FromBlank { get; private set; }
    public string ToChipDesignName { get; private set; } = default!;
    public bool AlwaysReadUid { get; private set; }
    public string RequiredVariablesJson { get; private set; } = "[]";
    public string RequiredKeyGroupsJson { get; private set; } = "[]";
    public string VariableConfigsJson { get; private set; } = "[]";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Transformation Create(
        string name,
        string? fromChipDesignName,
        bool fromBlank,
        string toChipDesignName,
        string requiredVariablesJson,
        string requiredKeyGroupsJson,
        string variableConfigsJson,
        DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        FromChipDesignName = NormalizeOptional(fromChipDesignName),
        FromBlank = fromBlank,
        ToChipDesignName = toChipDesignName.Trim(),
        AlwaysReadUid = true,
        RequiredVariablesJson = requiredVariablesJson,
        RequiredKeyGroupsJson = requiredKeyGroupsJson,
        VariableConfigsJson = variableConfigsJson,
        CreatedAt = now,
        UpdatedAt = now
    };

    public void Update(string name, string? fromChipDesignName, bool fromBlank, string toChipDesignName, string requiredVariablesJson, string requiredKeyGroupsJson, string variableConfigsJson, DateTimeOffset now)
    {
        Name = name.Trim();
        FromChipDesignName = NormalizeOptional(fromChipDesignName);
        FromBlank = fromBlank;
        ToChipDesignName = toChipDesignName.Trim();
        RequiredVariablesJson = requiredVariablesJson;
        RequiredKeyGroupsJson = requiredKeyGroupsJson;
        VariableConfigsJson = variableConfigsJson;
        UpdatedAt = now;
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
