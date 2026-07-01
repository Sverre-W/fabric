using Fabric.Hardware.Contracts.Inventory;

namespace Fabric.Hardware.Agent.Devices;

public interface IQrReaderDevice
{
    string DeviceId { get; }

    HardwareDeviceInventoryItem GetInventoryItem();

    Task<string> ReadQrCodeAsync(CancellationToken cancellationToken);

    IAsyncEnumerable<string> ReadNotificationsAsync(CancellationToken cancellationToken);
}
