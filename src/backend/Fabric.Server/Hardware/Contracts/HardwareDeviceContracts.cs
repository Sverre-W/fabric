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
    DateTimeOffset LastSeenAt,
    HardwareConnectionStatus ConnectionStatus,
    bool IsAvailable,
    HardwareDeviceAvailabilityReason? AvailabilityReason);

public sealed record HardwareDeviceHealthResponse(
    string AgentId,
    string DeviceId,
    string State,
    bool Enabled,
    DateTimeOffset LastSeenAt,
    string DiagnosticsJson,
    HardwareConnectionStatus ConnectionStatus,
    bool IsAvailable,
    HardwareDeviceAvailabilityReason? AvailabilityReason);

public enum HardwareDeviceAvailabilityReason
{
    DeviceDisabled,
    DeviceOffline
}

public static class HardwareDeviceMapper
{
    public static HardwareDeviceResponse ToResponse(this HardwareDevice device, HardwareConnectionStatus connectionStatus, bool isAvailable, HardwareDeviceAvailabilityReason? availabilityReason) => new(
        device.AgentId,
        device.DeviceId,
        device.Kind,
        device.Driver,
        device.Capabilities,
        device.State,
        device.Enabled,
        device.LastSeenAt,
        connectionStatus,
        isAvailable,
        availabilityReason);

    public static HardwareDeviceHealthResponse ToHealthResponse(this HardwareDevice device, HardwareConnectionStatus connectionStatus, bool isAvailable, HardwareDeviceAvailabilityReason? availabilityReason) => new(
        device.AgentId,
        device.DeviceId,
        device.State,
        device.Enabled,
        device.LastSeenAt,
        device.DiagnosticsJson,
        connectionStatus,
        isAvailable,
        availabilityReason);
}
