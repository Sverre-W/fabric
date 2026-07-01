using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent.Devices;

public interface IRfidReaderDevice
{
    string DeviceId { get; }

    HardwareDeviceInventoryItem GetInventoryItem();

    Task<string> ReadCardAsync(CancellationToken cancellationToken);
}
