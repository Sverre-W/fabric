namespace Fabric.Hardware.Contracts;

public enum HardwareOperationStatus
{
    Succeeded,
    Timeout,
    Cancelled,
    DeviceUnavailable,
    Busy,
    Failed
}
