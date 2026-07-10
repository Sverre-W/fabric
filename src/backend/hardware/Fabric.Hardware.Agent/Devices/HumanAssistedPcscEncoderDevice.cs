using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Inventory;
namespace Fabric.Hardware.Agent.Devices;

public sealed class HumanAssistedPcscEncoderDevice(HumanAssistedEncoderOptions options, ILogger<HumanAssistedPcscEncoderDevice> logger) : PcscEncoderDeviceBase(options.Reader, options.Implementation)
{
    public override string DeviceId => options.DeviceId;

    public override HardwareDeviceInventoryItem GetInventoryItem()
    {
        bool detected = ReaderExists();

        return new HardwareDeviceInventoryItem(
            options.DeviceId,
            "encoder",
            "pcsc",
            [HardwareCapabilities.CardPresent, HardwareCapabilities.RfidApduExchange, HardwareCapabilities.CardEject],
            detected ? "online" : "offline",
            new HardwareDeviceDiagnostics(options.Reader, Configured: !string.IsNullOrWhiteSpace(options.Reader), Detected: detected, Platform: Environment.OSVersion.Platform.ToString()));
    }

    public override async Task WaitForCardPresentAsync(CancellationToken cancellationToken)
    {
        logger.EncoderWaitingForCardPresent(options.DeviceId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!ReaderExists())
                    throw new InvalidOperationException("Configured PCSC reader is not available.");

                if (IsCardPresent())
                {
                    logger.EncoderCardPresentDetected(options.DeviceId);
                    EnsureSession();
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("Configured PCSC reader is not available.", ex);
        }
    }

    public override async Task WaitForCardRemovalAsync(CancellationToken cancellationToken)
    {
        logger.EncoderWaitingForCardRemoval(options.DeviceId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!ReaderExists() || !IsCardPresent())
                {
                    logger.EncoderCardRemovalDetected(options.DeviceId);
                    DisposeSession();
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException("Configured PCSC reader is not available.", ex);
        }
    }
}
