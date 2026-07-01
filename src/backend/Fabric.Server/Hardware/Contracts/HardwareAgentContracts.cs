using Fabric.Server.Hardware.Domain;
using Riok.Mapperly.Abstractions;

namespace Fabric.Server.Hardware.Contracts;

public sealed record HardwareAgentResponse(
    string Id,
    string Name,
    bool Enabled,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? LastInventoryAt);

public sealed record CreateHardwareAgentRequest(
    string Id,
    string Name);

public sealed record UpdateHardwareAgentRequest(
    string Name,
    bool Enabled);

public sealed record HardwareAgentKeyResponse(
    HardwareAgentResponse Agent,
    string ApiKey);

[Mapper]
public static partial class HardwareAgentMapper
{
    [MapperIgnoreSource(nameof(HardwareAgent.ApiKeyHash))]
    [MapperIgnoreSource(nameof(HardwareAgent.ApiKeySalt))]
    public static partial HardwareAgentResponse ToResponse(this HardwareAgent agent);
}
