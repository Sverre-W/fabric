using Fabric.Server.Sagas.VisitorPreOnboarding;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public class VisitorPreOnboardingWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<VisitorPreOnboardingWorker> logger,
    TimeProvider timeProvider) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval, timeProvider);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessSagasAsync(stoppingToken);
            await ExpireSagasAsync(stoppingToken);
        }
    }

    private async Task ProcessSagasAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        VisitorPreOnboardingSagaService service = scope.ServiceProvider.GetRequiredService<VisitorPreOnboardingSagaService>();

        IReadOnlyList<VisitorPreOnboardingSaga> sagas = await service.GetRetryableAsync(cancellationToken);

        foreach (VisitorPreOnboardingSaga saga in sagas)
        {
            try
            {
                SagaStepResult result = await service.StepAsync(saga, cancellationToken);

                if (result == SagaStepResult.Continue && IsRetryableState(saga.State))
                {
                    IReadOnlyList<VisitorPreOnboardingSaga> updated = await service.GetRetryableAsync(cancellationToken);
                    VisitorPreOnboardingSaga? same = updated.FirstOrDefault(x => x.Id == saga.Id);
                    if (same is not null)
                        await service.StepAsync(same, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing saga {SagaId} in state {State}", saga.Id, saga.State);
            }
        }
    }

    private async Task ExpireSagasAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        VisitorPreOnboardingSagaService service = scope.ServiceProvider.GetRequiredService<VisitorPreOnboardingSagaService>();

        int count = await service.ExpirePassedSagasAsync(cancellationToken);
        if (count > 0)
            logger.LogInformation("Expired {Count} sagas past their visit date", count);
    }

    private static bool IsRetryableState(VisitorPreOnboardingState state) =>
        state is VisitorPreOnboardingState.RegisteringArrival
            or VisitorPreOnboardingState.GeneratingQr
            or VisitorPreOnboardingState.UpdatingArrivalQr
            or VisitorPreOnboardingState.SendingInvitation;
}
