namespace Fabric.Hardware.Contracts.Inventory;

public sealed record HardwareDeviceDiagnostics(
    string? Connection,
    bool Configured,
    bool Detected,
    string? Platform);
