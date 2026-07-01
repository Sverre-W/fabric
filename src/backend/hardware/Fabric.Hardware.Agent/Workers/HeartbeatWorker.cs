using Fabric.Hardware.Agent.Gateway;
using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts.Agents;
using Microsoft.Extensions.Options;

namespace Fabric.Hardware.Agent;

public sealed class HeartbeatWorker(HardwareGatewayClient gatewayClient, TimeProvider timeProvider, IOptions<HardwareAgentOptions> options, ILogger<HeartbeatWorker> logger) : BackgroundService
{
    private readonly HardwareAgentOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.WorkerStarted(nameof(HeartbeatWorker));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await gatewayClient.PostHeartbeatAsync(new PostHardwareAgentHeartbeatRequest(timeProvider.GetUtcNow()), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.HeartbeatFailed(ex);
            }

            await Task.Delay(_options.HeartbeatInterval, stoppingToken);
        }

        logger.WorkerStopped(nameof(HeartbeatWorker));
    }
}
