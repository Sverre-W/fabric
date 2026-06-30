using System.Text.Json.Nodes;

namespace Fabric.Server.Hardware.Domain;

public class HardwareEventInboxItem
{
    public Guid EventId { get; private set; }
    public string AgentId { get; private set; } = default!;
    public string DeviceId { get; private set; } = default!;
    public string Type { get; private set; } = default!;
    public DateTimeOffset OccurredAt { get; private set; }
    public string PayloadJson { get; private set; } = "{}";
    public DateTimeOffset ReceivedAt { get; private set; }

    public static HardwareEventInboxItem Create(
        Guid eventId,
        string agentId,
        string deviceId,
        string type,
        DateTimeOffset occurredAt,
        JsonObject? payload,
        DateTimeOffset receivedAt) => new()
    {
        EventId = eventId,
        AgentId = HardwareAgent.NormalizeId(agentId),
        DeviceId = HardwareDevice.NormalizeDeviceId(deviceId),
        Type = type.Trim(),
        OccurredAt = occurredAt,
        PayloadJson = payload?.ToJsonString() ?? "{}",
        ReceivedAt = receivedAt
    };
}
