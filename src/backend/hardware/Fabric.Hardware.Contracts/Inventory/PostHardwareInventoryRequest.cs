namespace Fabric.Hardware.Contracts.Inventory;

public sealed record PostHardwareInventoryRequest(
    DateTimeOffset ReportedAt,
    IReadOnlyList<HardwareDeviceInventoryItem> Devices);
