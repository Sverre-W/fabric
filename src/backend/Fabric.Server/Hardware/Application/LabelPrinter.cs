using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Contracts.Labels;

namespace Fabric.Server.Hardware.Application;

public interface ILabelPrinter
{
    Task<LabelPrintResponse> PrintAsync(HardwareDeviceRef device, PrintLabelRequest request, CancellationToken cancellationToken);
}

public sealed class LabelPrinter(
    HardwareCommandStore commandStore,
    HardwareAgentConnectionManager connectionManager) : ILabelPrinter
{
    private static readonly TimeSpan PrintTimeout = TimeSpan.FromSeconds(15);

    public async Task<LabelPrintResponse> PrintAsync(
        HardwareDeviceRef device,
        PrintLabelRequest request,
        CancellationToken cancellationToken)
    {
        var payload = new System.Text.Json.Nodes.JsonObject
        {
            ["template"] = request.Template,
            ["copies"] = request.Copies,
            ["data"] = request.Data.DeepClone()
        };

        PendingHardwareCommand command = commandStore.Create(device.AgentId, device.DeviceId, HardwareCapabilities.LabelPrint, payload, PrintTimeout);
        connectionManager.NotifyCommandAvailable(device.AgentId, command.CommandId);

        PostHardwareCommandResultRequest result = await commandStore.WaitForResultAsync(command, cancellationToken);
        return new LabelPrintResponse(result.Status, result.Error);
    }
}
