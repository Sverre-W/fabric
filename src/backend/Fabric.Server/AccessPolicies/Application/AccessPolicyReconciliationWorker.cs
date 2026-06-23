using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Tenants.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies.Application;

public sealed class AccessPolicyReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    AccessPolicyReconciliationTrigger trigger,
    ILogger<AccessPolicyReconciliationWorker> logger,
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

                await ProcessTriggeredReconciliationsAsync(stoppingToken);
                triggerReady = trigger.WaitToReadAsync(stoppingToken).AsTask();
            }

            if (completed == timerReady)
            {
                if (!await timerReady)
                    break;

                await ProcessDueReconciliationsAsync(stoppingToken);
                timerReady = timer.WaitForNextTickAsync(stoppingToken).AsTask();
            }
        }
    }

    private async Task ProcessTriggeredReconciliationsAsync(CancellationToken cancellationToken)
    {
        while (trigger.TryRead(out AccessPolicyReconciliationWorkItem? workItem) && workItem is not null)
            await ProcessReconciliationAsync(workItem, cancellationToken);
    }

    private async Task ProcessDueReconciliationsAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        TenantsDbContext tenantsDb = scope.ServiceProvider.GetRequiredService<TenantsDbContext>();
        List<string> tenantIds = await tenantsDb.Tenants
            .AsNoTracking()
            .Select(tenant => tenant.Id)
            .ToListAsync(cancellationToken);

        foreach (string tenantId in tenantIds)
            await ProcessDueReconciliationsAsync(tenantId, cancellationToken);
    }

    private async Task ProcessDueReconciliationsAsync(string tenantId, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            if (!await SetTenantAsync(scope.ServiceProvider, tenantId, cancellationToken))
                return;

            AccessPolicyService service = scope.ServiceProvider.GetRequiredService<AccessPolicyService>();
            IReadOnlyList<AccessPolicyReconciliationWorkItem> workItems = await service.GetPendingReconciliationWorkItemsAsync(cancellationToken);

            foreach (AccessPolicyReconciliationWorkItem workItem in workItems)
                await ProcessReconciliationAsync(workItem, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing due access policy reconciliations for tenant {TenantId}", tenantId);
        }
    }

    private async Task ProcessReconciliationAsync(AccessPolicyReconciliationWorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            if (!await SetTenantAsync(scope.ServiceProvider, workItem.TenantId, cancellationToken))
                return;

            AccessPolicyService service = scope.ServiceProvider.GetRequiredService<AccessPolicyService>();
            await service.ReconcileSubjectSystemPolicies(workItem.SubjectId, workItem.SystemId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error reconciling access policies for subject {SubjectId}, system {SystemId}, tenant {TenantId}",
                workItem.SubjectId,
                workItem.SystemId,
                workItem.TenantId);
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
