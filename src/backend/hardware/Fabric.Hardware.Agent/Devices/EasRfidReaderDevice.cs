using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Inventory;
using Fabric.Hardware.RfidEas.Infrastructure;

namespace Fabric.Hardware.Agent.Devices;

public sealed class EasRfidReaderDevice(RfidEasReaderOptions options, Func<RfidEasReader> readerFactory, ILogger<EasRfidReaderDevice> logger) : IRfidReaderDevice
{
    public string DeviceId => options.DeviceId;

    public HardwareDeviceInventoryItem GetInventoryItem()
    {
        bool detected = TryEnsureReader();
        return new HardwareDeviceInventoryItem(
            options.DeviceId,
            "rfid-reader",
            "eas-pcprox",
            [HardwareCapabilities.RfidRead],
            detected ? "online" : "offline",
            new HardwareDeviceDiagnostics($"logical-reader:{options.ReaderId}", Configured: true, Detected: detected, Platform: Environment.OSVersion.Platform.ToString()));
    }

    public async Task<string> ReadCardAsync(CancellationToken cancellationToken)
    {
        try
        {
            RfidEasReader reader = readerFactory();
            return await reader.ReadCard(options.ReaderId, cancellationToken);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException or TypeInitializationException)
        {
            logger.RfidEasUnavailable(options.DeviceId, ex);
            throw new InvalidOperationException("RFID EAS reader is not available.", ex);
        }
    }

    private bool TryEnsureReader()
    {
        try
        {
            readerFactory();
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or InvalidOperationException or TypeInitializationException)
        {
            logger.RfidEasUnavailable(options.DeviceId, ex);
            return false;
        }
    }
}
