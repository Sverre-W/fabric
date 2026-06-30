namespace Fabric.Hardware.Contracts.Inventory;

public sealed record HardwareDeviceInventoryItem(
    string DeviceId,
    string Kind,
    string Driver,
    IReadOnlyList<string> Capabilities,
    string State,
    HardwareDeviceDiagnostics Diagnostics);
