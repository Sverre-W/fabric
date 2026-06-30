using System.Text.Json.Nodes;

namespace Fabric.Hardware.Contracts.Commands;

public sealed record HardwareCommandEnvelope(
    Guid CommandId,
    string DeviceId,
    string Capability,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    JsonObject? Payload);
