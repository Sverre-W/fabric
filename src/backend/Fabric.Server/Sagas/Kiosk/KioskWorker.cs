using Fabric.Server.Infrastructure.Tenancy;

namespace Fabric.Server.Sagas.Kiosk;

public sealed class KioskWorker(
    IServiceScopeFactory scopeFactory,
    KioskSagaTrigger trigger,
    ILogger<KioskWorker> logger,
    TimeProvider timeProvider) : BackgroundService
{
    private const int EventBatchSize = 100;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval, timeProvider);
        Task<bool> triggerReady = trigger.WaitToReadAsync(stoppingToken).AsTask();
        Task<bool> timerReady = timer.WaitForNextTickAsync(stoppingToken).AsTask();

        while (!stoppingToken.IsCancellationRequested)
        {
            Task<bool> completed = await Task.WhenAny(triggerReady, timerReady);

            if (completed == triggerReady)
            {
                if (!await triggerReady)
                    break;

                while (trigger.TryRead()) { }
                await ProcessDueEventsAsync(stoppingToken);
                triggerReady = trigger.WaitToReadAsync(stoppingToken).AsTask();
            }

            if (completed == timerReady)
            {
                if (!await timerReady)
                    break;

                await ProcessDueEventsAsync(stoppingToken);
                timerReady = timer.WaitForNextTickAsync(stoppingToken).AsTask();
            }
        }
    }

    private async Task ProcessDueEventsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            KioskSagaService service = scope.ServiceProvider.GetRequiredService<KioskSagaService>();
            IReadOnlyList<KioskSagaEventWorkItem> workItems = await service.GetDueEventWorkItemsAsync(EventBatchSize, cancellationToken);

            foreach (KioskSagaEventWorkItem workItem in workItems)
                await ProcessEventAsync(workItem, cancellationToken);

            if (workItems.Count < EventBatchSize)
                break;
        }
    }

    private async Task ProcessEventAsync(KioskSagaEventWorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            if (!await SetTenantAsync(scope.ServiceProvider, workItem.TenantId, cancellationToken))
                return;

            KioskSagaService service = scope.ServiceProvider.GetRequiredService<KioskSagaService>();
            await service.ProcessEventAsync(workItem.EventId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing kiosk saga event {EventId} for tenant {TenantId}", workItem.EventId, workItem.TenantId);
        }
    }

    private static async Task<bool> SetTenantAsync(IServiceProvider serviceProvider, string tenantId, CancellationToken cancellationToken)
    {
        ITenantStore tenantStore = serviceProvider.GetRequiredService<ITenantStore>();
        TenantInfo? tenant = await tenantStore.GetTenantAsync(tenantId, cancellationToken);
        if (tenant is null)
            return false;

        ITenantContextAccessor tenantContext = serviceProvider.GetRequiredService<ITenantContextAccessor>();
        tenantContext.SetTenant(tenant);
        return true;
    }
}
