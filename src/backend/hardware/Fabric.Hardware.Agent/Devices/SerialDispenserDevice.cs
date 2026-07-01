using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Inventory;
using Fabric.Hardware.Dispenser;

namespace Fabric.Hardware.Agent.Devices;

public sealed class SerialDispenserDevice(DispenserDeviceOptions options, ILogger<DispenserSerialPort> dispenserLogger, ILogger<SerialDispenserDevice> logger) : IDispenserDevice, IDisposable
{
    private readonly object _gate = new();
    private DispenserSerialPort? _dispenser;
    private bool _disposed;

    public string DeviceId => options.DeviceId;

    public string RfidReaderDeviceId => options.RfidReaderDeviceId;

    public TimeSpan RfidReadTimeout => options.RfidReadTimeout;

    public HardwareDeviceInventoryItem GetInventoryItem()
    {
        bool detected = TryEnsureOpen();
        return new HardwareDeviceInventoryItem(
            options.DeviceId,
            "card-dispenser",
            "serial",
            [HardwareCapabilities.CardPresent, HardwareCapabilities.CardDrop, HardwareCapabilities.CardDispense],
            detected ? "online" : "offline",
            new HardwareDeviceDiagnostics(options.ComPort, Configured: !string.IsNullOrWhiteSpace(options.ComPort), Detected: detected, Platform: Environment.OSVersion.Platform.ToString()));
    }

    public async Task<bool> DispenseAsync(CancellationToken cancellationToken)
    {
        DispenserSerialPort dispenser = EnsureOpen();
        return await dispenser.Dispense(cancellationToken);
    }

    public async Task<bool> DropAsync(CancellationToken cancellationToken)
    {
        DispenserSerialPort dispenser = EnsureOpen();
        return await dispenser.Drop(cancellationToken);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _dispenser?.Dispose();
            _dispenser = null;
        }
    }

    private bool TryEnsureOpen()
    {
        try
        {
            EnsureOpen();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.DispenserOpenFailed(options.DeviceId, options.ComPort, ex);
            return false;
        }
    }

    private DispenserSerialPort EnsureOpen()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_dispenser is not null)
                return _dispenser;

            _dispenser = new DispenserSerialPort(dispenserLogger, options.ComPort, options.ResponseTimeout);
            logger.DispenserOpened(options.DeviceId, options.ComPort);
            return _dispenser;
        }
    }
}
