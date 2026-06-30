using System.Text.Json.Nodes;
using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Contracts.Qr;

namespace Fabric.Server.Hardware.Application;

public interface IQrScanner
{
    Task<QrScanResponse> ScanAsync(HardwareDeviceRef device, CancellationToken cancellationToken);
}

public sealed class QrScanner(
    HardwareCommandStore commandStore,
    HardwareAgentConnectionManager connectionManager) : IQrScanner
{
    private static readonly TimeSpan ScanTimeout = TimeSpan.FromSeconds(30);

    public async Task<QrScanResponse> ScanAsync(HardwareDeviceRef device, CancellationToken cancellationToken)
    {
        PendingHardwareCommand command = commandStore.Create(device.AgentId, device.DeviceId, HardwareCapabilities.QrScan, null, ScanTimeout);
        connectionManager.NotifyCommandAvailable(device.AgentId, command.CommandId);

        PostHardwareCommandResultRequest result = await commandStore.WaitForResultAsync(command, cancellationToken);
        string? value = result.Result?["value"]?.GetValue<string>();

        return new QrScanResponse(result.Status, value, result.Error);
    }
}
