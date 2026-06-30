namespace Fabric.Hardware.Contracts.Commands;

public enum HardwareCommandStatus
{
    Pending,
    Claimed,
    Running,
    Succeeded,
    Timeout,
    Cancelled,
    DeviceUnavailable,
    Busy,
    Failed,
    Expired
}
