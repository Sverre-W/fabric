using Fabric.Hardware.Desfire.Encoding.Models;

namespace Fabric.Server.Desfire.Domain;

public sealed class KeyDiversificationStrategyEntity
{
    private KeyDiversificationStrategyEntity() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public KeyDiversificationAlgorithm Algorithm { get; private set; }
    public string InputsJson { get; private set; } = "[]";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static KeyDiversificationStrategyEntity Create(string name, KeyDiversificationAlgorithm algorithm, string inputsJson, DateTimeOffset now) => new()
    {
        Id = Guid.NewGuid(),
        Name = name.Trim(),
        Algorithm = algorithm,
        InputsJson = inputsJson,
        CreatedAt = now,
        UpdatedAt = now
    };

    public void Update(string name, KeyDiversificationAlgorithm algorithm, string inputsJson, DateTimeOffset now)
    {
        Name = name.Trim();
        Algorithm = algorithm;
        InputsJson = inputsJson;
        UpdatedAt = now;
    }
}
