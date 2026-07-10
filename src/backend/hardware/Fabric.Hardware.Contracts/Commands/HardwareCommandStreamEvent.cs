namespace Fabric.Hardware.Contracts.Commands;

public sealed record HardwareCommandStreamEvent(
    HardwareCommandEventType Type,
    Guid CommandId,
    string? DeviceId,
    string? Capability,
    string? Reason);
