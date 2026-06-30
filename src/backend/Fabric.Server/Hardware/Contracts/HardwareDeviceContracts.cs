using Fabric.Server.Hardware.Domain;

namespace Fabric.Server.Hardware.Contracts;

public sealed record HardwareDeviceResponse(
    string AgentId,
    string DeviceId,
    string Kind,
    string Driver,
    IReadOnlyList<string> Capabilities,
    string State,
    bool Enabled,
    DateTimeOffset LastSeenAt);

public sealed record HardwareDeviceHealthResponse(
    string AgentId,
    string DeviceId,
    string State,
    bool Enabled,
    DateTimeOffset LastSeenAt,
    string DiagnosticsJson);

public static class HardwareDeviceMapper
{
    public static HardwareDeviceResponse ToResponse(this HardwareDevice device) => new(
        device.AgentId,
        device.DeviceId,
        device.Kind,
        device.Driver,
        device.Capabilities,
        device.State,
        device.Enabled,
        device.LastSeenAt);

    public static HardwareDeviceHealthResponse ToHealthResponse(this HardwareDevice device) => new(
        device.AgentId,
        device.DeviceId,
        device.State,
        device.Enabled,
        device.LastSeenAt,
        device.DiagnosticsJson);
}
