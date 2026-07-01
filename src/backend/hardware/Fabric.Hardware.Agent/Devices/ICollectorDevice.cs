using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent.Devices;

public interface ICollectorDevice
{
    string DeviceId { get; }

    string RfidReaderDeviceId { get; }

    TimeSpan RfidReadTimeout { get; }

    TimeSpan RemovalTimeout { get; }

    HardwareDeviceInventoryItem GetInventoryItem();

    Task WaitForCardAtReaderAsync(CancellationToken cancellationToken);

    Task<bool> CollectAsync(bool placeInCollectorStack, CancellationToken cancellationToken);

    Task<bool> EjectAsync(CancellationToken cancellationToken);

    Task WaitForCardRemovalAsync(CancellationToken cancellationToken);
}
