using Fabric.Server.Hardware.Domain;
using Fabric.Server.Infrastructure.Tenancy;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireEncodingWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<DesfireEncodingOptions> options,
    ILogger<DesfireEncodingWorker> logger) : BackgroundService
{
    private readonly string _workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";
    private int _tenantCursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.DesfireEncodingWorkerStarted(_workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.DesfireEncodingWorkerFailed(_workerId, ex);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(options.Value.SchedulerIntervalMilliseconds), stoppingToken);
        }

        logger.DesfireEncodingWorkerStopped(_workerId);
    }

    private async Task DispatchOnceAsync(CancellationToken ct)
    {
        List<TenantInfo> tenants;
        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            tenants = await scope.ServiceProvider.GetRequiredService<ITenantStore>().GetAllTenantsAsync(ct);
        }

        if (tenants.Count == 0)
            return;

        List<Task> running = [];
        foreach (TenantInfo tenant in Rotate(tenants))
        {
            if (running.Count >= options.Value.MaxConcurrentRuns)
                break;

            using IServiceScope scope = scopeFactory.CreateScope();
            scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>().SetTenant(tenant);
            DesfireEncodingService encodingService = scope.ServiceProvider.GetRequiredService<DesfireEncodingService>();
            List<HardwareDevice> devices = await encodingService.GetReadyEncodingDevicesAsync(ct);

            foreach (HardwareDevice device in devices)
            {
                if (running.Count >= options.Value.MaxConcurrentRuns)
                    break;

                Domain.EncodingRun? run = await encodingService.TryClaimNextRunForDeviceAsync(device.AgentId, device.DeviceId, _workerId, ct);
                if (run is null)
                    continue;

                running.Add(ExecuteInTenantScopeAsync(tenant, run.Id, device.AgentId, device.DeviceId, ct));
            }
        }

        if (running.Count > 0)
            await Task.WhenAll(running);
    }

    private async Task ExecuteInTenantScopeAsync(TenantInfo tenant, Guid runId, string agentId, string deviceId, CancellationToken ct)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<ITenantContextAccessor>().SetTenant(tenant);
        DesfireEncodingService encodingService = scope.ServiceProvider.GetRequiredService<DesfireEncodingService>();
        await encodingService.ExecuteRunAsync(runId, agentId, deviceId, requeueOnTransientFailure: true, ct);
    }

    private IEnumerable<TenantInfo> Rotate(IReadOnlyList<TenantInfo> tenants)
    {
        int start = Interlocked.Increment(ref _tenantCursor);
        for (int i = 0; i < tenants.Count; i++)
            yield return tenants[(start + i) % tenants.Count];
    }
}

internal static partial class DesfireEncodingWorkerLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Desfire encoding worker {WorkerId} started")]
    public static partial void DesfireEncodingWorkerStarted(this ILogger logger, string workerId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Desfire encoding worker {WorkerId} stopped")]
    public static partial void DesfireEncodingWorkerStopped(this ILogger logger, string workerId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Desfire encoding worker {WorkerId} failed")]
    public static partial void DesfireEncodingWorkerFailed(this ILogger logger, string workerId, Exception exception);
}
