using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent.Devices;

public interface IDispenserDevice
{
    string DeviceId { get; }

    string RfidReaderDeviceId { get; }

    TimeSpan RfidReadTimeout { get; }

    HardwareDeviceInventoryItem GetInventoryItem();

    Task<bool> DispenseAsync(CancellationToken cancellationToken);

    Task<bool> DropAsync(CancellationToken cancellationToken);
}
