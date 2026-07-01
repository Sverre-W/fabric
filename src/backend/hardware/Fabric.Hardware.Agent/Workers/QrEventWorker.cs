using System.Text.Json.Nodes;
using Fabric.Hardware.Agent.Devices;
using Fabric.Hardware.Agent.Gateway;
using Fabric.Hardware.Contracts.Events;

namespace Fabric.Hardware.Agent;

public sealed class QrEventWorker(HardwareGatewayClient gatewayClient, IReadOnlyList<IQrReaderDevice> qrReaders, TimeProvider timeProvider, ILogger<QrEventWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.WorkerStarted(nameof(QrEventWorker));

        Task[] tasks = qrReaders.Select(qrReader => RunQrReaderAsync(qrReader, stoppingToken)).ToArray();
        await Task.WhenAll(tasks);

        logger.WorkerStopped(nameof(QrEventWorker));
    }

    private async Task RunQrReaderAsync(IQrReaderDevice qrReader, CancellationToken stoppingToken)
    {
        try
        {
            await foreach (string reading in qrReader.ReadNotificationsAsync(stoppingToken))
            {
                try
                {
                    var payload = new JsonObject { ["value"] = reading };
                    var request = new PostHardwareEventRequest(Guid.NewGuid(), timeProvider.GetUtcNow(), qrReader.DeviceId, "qr.scanned", payload);
                    await gatewayClient.PostEventAsync(request, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.QrEventPostFailed(qrReader.DeviceId, ex);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
