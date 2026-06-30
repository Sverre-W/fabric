using Fabric.Hardware.Contracts;

namespace Fabric.Hardware.Contracts.Labels;

public sealed record LabelPrintResponse(
    HardwareOperationStatus Status,
    HardwareErrorResponse? Error);
