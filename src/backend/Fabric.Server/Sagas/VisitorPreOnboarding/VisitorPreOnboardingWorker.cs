using Fabric.Server.Infrastructure.Tenancy;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public class VisitorPreOnboardingWorker(
    IServiceScopeFactory scopeFactory,
    VisitorPreOnboardingSagaTrigger trigger,
    ILogger<VisitorPreOnboardingWorker> logger,
    TimeProvider timeProvider) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

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

                await ProcessTriggeredSagasAsync(stoppingToken);
                triggerReady = trigger.WaitToReadAsync(stoppingToken).AsTask();
            }

            if (completed == timerReady)
            {
                if (!await timerReady)
                    break;

                await ProcessDueSagasAsync(stoppingToken);
                await ExpireSagasAsync(stoppingToken);
                timerReady = timer.WaitForNextTickAsync(stoppingToken).AsTask();
            }
        }
    }

    private async Task ProcessTriggeredSagasAsync(CancellationToken cancellationToken)
    {
        while (trigger.TryRead(out VisitorPreOnboardingSagaWorkItem? workItem) && workItem is not null)
            await ProcessSagaAsync(workItem, cancellationToken);
    }

    private async Task ProcessDueSagasAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        VisitorPreOnboardingSagaService service = scope.ServiceProvider.GetRequiredService<VisitorPreOnboardingSagaService>();

        IReadOnlyList<VisitorPreOnboardingSagaWorkItem> workItems = await service.GetRetryableWorkItemsAsync(cancellationToken);

        foreach (VisitorPreOnboardingSagaWorkItem workItem in workItems)
            await ProcessSagaAsync(workItem, cancellationToken);
    }

    private async Task ProcessSagaAsync(VisitorPreOnboardingSagaWorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            if (!await SetTenantAsync(scope.ServiceProvider, workItem.TenantId, cancellationToken))
                return;

            VisitorPreOnboardingSagaService service = scope.ServiceProvider.GetRequiredService<VisitorPreOnboardingSagaService>();
            await service.ProcessAsync(workItem.SagaId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing saga {SagaId} for tenant {TenantId}", workItem.SagaId, workItem.TenantId);
        }
    }

    private async Task ExpireSagasAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        VisitorPreOnboardingSagaService service = scope.ServiceProvider.GetRequiredService<VisitorPreOnboardingSagaService>();

        int count = await service.ExpirePassedSagasForAllTenantsAsync(cancellationToken);
        if (count > 0)
            logger.LogInformation("Expired {Count} sagas past their visit date", count);
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
