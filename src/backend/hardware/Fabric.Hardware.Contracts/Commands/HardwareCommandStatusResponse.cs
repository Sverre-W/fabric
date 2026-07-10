namespace Fabric.Hardware.Contracts.Commands;

public sealed record HardwareCommandStatusResponse(
    Guid CommandId,
    string DeviceId,
    string Capability,
    HardwareCommandStatus Status,
    string? ErrorCode,
    string? ErrorMessage);
