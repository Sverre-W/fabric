using System.Text.Json.Nodes;

namespace Fabric.Hardware.Contracts.Events;

public sealed record PostHardwareEventRequest(
    Guid EventId,
    DateTimeOffset OccurredAt,
    string DeviceId,
    string Type,
    JsonObject? Payload);
