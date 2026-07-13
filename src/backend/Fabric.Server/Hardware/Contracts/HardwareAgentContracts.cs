using Fabric.Server.Hardware.Domain;

namespace Fabric.Server.Hardware.Contracts;

public sealed record HardwareAgentResponse(
    string Id,
    string Name,
    bool Enabled,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? LastInventoryAt,
    HardwareConnectionStatus ConnectionStatus);

public enum HardwareConnectionStatus
{
    Online,
    Stale,
    Offline
}

public sealed record CreateHardwareAgentRequest(
    string Id,
    string Name);

public sealed record UpdateHardwareAgentRequest(
    string Name,
    bool Enabled);

public sealed record HardwareAgentKeyResponse(
    HardwareAgentResponse Agent,
    string ApiKey);

public static class HardwareAgentMapper
{
    public static HardwareAgentResponse ToResponse(this HardwareAgent agent, HardwareConnectionStatus connectionStatus) => new(
        agent.Id,
        agent.Name,
        agent.Enabled,
        agent.LastSeenAt,
        agent.LastInventoryAt,
        connectionStatus);
}
