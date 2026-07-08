using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Inventory;
using Fabric.Hardware.Dispenser;

namespace Fabric.Hardware.Agent.Devices;

public sealed class DispenserEncoderDevice(DispenserEncoderOptions options, ILogger<DispenserSerialPort> dispenserLogger) : PcscEncoderDeviceBase(options.Reader, options.Implementation)
{
    private readonly object _gate = new();
    private DispenserSerialPort? _dispenser;

    public override string DeviceId => options.DeviceId;

    public override HardwareDeviceInventoryItem GetInventoryItem()
    {
        bool detected = ReaderExists() && TryEnsureDispenserOpen();
        return new HardwareDeviceInventoryItem(
            options.DeviceId,
            "encoder",
            "pcsc-dispenser",
            [HardwareCapabilities.CardPresent, HardwareCapabilities.RfidApduExchange, HardwareCapabilities.CardEject],
            detected ? "online" : "offline",
            new HardwareDeviceDiagnostics($"{options.ComPort} | {options.Reader}", Configured: !string.IsNullOrWhiteSpace(options.ComPort) && !string.IsNullOrWhiteSpace(options.Reader), Detected: detected, Platform: Environment.OSVersion.Platform.ToString()));
    }

    public override async Task WaitForCardPresentAsync(CancellationToken cancellationToken)
    {
        try
        {
            EnsureReaderAvailable();

            DispenserSerialPort dispenser = EnsureDispenserOpen();
            bool dispensed = await dispenser.Dispense(cancellationToken);
            if (!dispensed)
                throw new InvalidOperationException("Dispenser did not confirm card dispense.");

            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                EnsureReaderAvailable();

                if (IsCardPresent())
                {
                    EnsureSession();
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("Configured dispenser encoder is not available.", ex);
        }
    }

    public override async Task WaitForCardRemovalAsync(CancellationToken cancellationToken)
    {
        try
        {
            DispenserSerialPort dispenser = EnsureDispenserOpen();
            bool dropped = await dispenser.Drop(cancellationToken);
            if (!dropped)
                throw new InvalidOperationException("Dispenser did not confirm card drop.");

            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!ReaderExists() || !IsCardPresent())
                {
                    DisposeSession();
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("Configured dispenser encoder is not available.", ex);
        }
    }

    public override void Dispose()
    {
        lock (_gate)
        {
            _dispenser?.Dispose();
            _dispenser = null;
        }

        base.Dispose();
    }

    private bool TryEnsureDispenserOpen()
    {
        try
        {
            EnsureDispenserOpen();
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return false;
        }
    }

    private DispenserSerialPort EnsureDispenserOpen()
    {
        lock (_gate)
        {
            if (_dispenser is not null)
                return _dispenser;

            _dispenser = new DispenserSerialPort(dispenserLogger, options.ComPort, options.ResponseTimeout);
            return _dispenser;
        }
    }

    private void EnsureReaderAvailable()
    {
        if (!ReaderExists())
            throw new InvalidOperationException("Configured PCSC reader is not available.");
    }
}
