using System.Text.Json.Nodes;
using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Server.Hardware.Application;

namespace Fabric.Server.Desfire.Application;

public sealed class HardwareRfidEncoder(HardwareCommandStore commandStore, HardwareAgentConnectionManager connectionManager, HardwareDeviceRef device)
    : IRfidEncoder
{
    private static readonly TimeSpan CommandTimeout = TimeSpan.FromSeconds(30);

    public async Task<byte[]> Send(byte[] data, CancellationToken cancellationToken = default)
    {
        var payload = new JsonObject { ["commandHex"] = Convert.ToHexString(data) };
        PendingHardwareCommand command = commandStore.Create(device.AgentId, device.DeviceId, HardwareCapabilities.RfidApduExchange, payload, CommandTimeout);
        connectionManager.NotifyCommandAvailable(device.AgentId, command.CommandId);

        PostHardwareCommandResultRequest result = await commandStore.WaitForResultAsync(command, cancellationToken);
        if (result.Status != HardwareOperationStatus.Succeeded)
            throw new InvalidOperationException(result.Error?.Message ?? $"RFID APDU command failed with status {result.Status}.");

        string? responseHex = result.Result?["responseHex"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(responseHex))
            throw new InvalidOperationException("RFID APDU response did not include responseHex.");

        return Convert.FromHexString(responseHex);
    }

    public void Dispose()
    {
    }
}
