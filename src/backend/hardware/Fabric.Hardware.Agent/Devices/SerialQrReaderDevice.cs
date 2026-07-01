using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Inventory;
using System.Runtime.CompilerServices;
using SerialQrReader = Fabric.Hardware.QrReader.QrReader;

namespace Fabric.Hardware.Agent.Devices;

public sealed class SerialQrReaderDevice(QrReaderDeviceOptions options, ILogger<SerialQrReaderDevice> logger) : IQrReaderDevice, IDisposable
{
    private readonly object _gate = new();
    private SerialQrReader? _reader;
    private bool _disposed;

    public string DeviceId => options.DeviceId;

    public HardwareDeviceInventoryItem GetInventoryItem()
    {
        bool detected = TryEnsureOpen();
        return new HardwareDeviceInventoryItem(
            options.DeviceId,
            "qr-reader",
            "serial",
            [HardwareCapabilities.QrScan],
            detected ? "online" : "offline",
            new HardwareDeviceDiagnostics(options.ComPort, Configured: !string.IsNullOrWhiteSpace(options.ComPort), Detected: detected, Platform: Environment.OSVersion.Platform.ToString()));
    }

    public Task<string> ReadQrCodeAsync(CancellationToken cancellationToken)
    {
        SerialQrReader reader = EnsureOpen();
        return reader.ReadQrCode(cancellationToken);
    }

    public async IAsyncEnumerable<string> ReadNotificationsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            SerialQrReader reader;
            try
            {
                reader = EnsureOpen();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
            {
                logger.QrReaderOpenFailed(options.DeviceId, options.ComPort, ex);
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                continue;
            }

            await foreach (string reading in reader.QrReadingNotifications(cancellationToken))
                yield return reading;
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _reader?.Dispose();
            _reader = null;
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
            logger.QrReaderOpenFailed(options.DeviceId, options.ComPort, ex);
            return false;
        }
    }

    private SerialQrReader EnsureOpen()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_reader is not null)
                return _reader;

            _reader = new SerialQrReader(options.ComPort);
            logger.QrReaderOpened(options.DeviceId, options.ComPort);
            return _reader;
        }
    }
}
