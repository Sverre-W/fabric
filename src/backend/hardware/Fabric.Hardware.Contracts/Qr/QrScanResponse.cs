using Fabric.Hardware.Contracts;

namespace Fabric.Hardware.Contracts.Qr;

public sealed record QrScanResponse(
    HardwareOperationStatus Status,
    string? Value,
    HardwareErrorResponse? Error);
