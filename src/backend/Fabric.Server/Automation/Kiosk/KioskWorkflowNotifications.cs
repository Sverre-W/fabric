using Elsa.Mediator.Contracts;
using Elsa.Workflows;
using Elsa.Workflows.Notifications;
using Fabric.Server.Sagas.Kiosk;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskWorkflowFinishedHandler(KioskSagaService sagaService) : INotificationHandler<WorkflowFinished>
{
    public async Task HandleAsync(WorkflowFinished notification, CancellationToken cancellationToken)
    {
        Guid? sessionId = GetSessionId(notification.WorkflowExecutionContext.CorrelationId, notification.WorkflowState.CorrelationId);
        if (sessionId is null)
            return;

        await sagaService.HandleWorkflowFinishedAsync(sessionId.Value, cancellationToken);
    }

    private static Guid? GetSessionId(string? workflowExecutionContextCorrelationId, string? workflowStateCorrelationId)
    {
        if (Guid.TryParse(workflowExecutionContextCorrelationId, out Guid sessionId))
            return sessionId;
        if (Guid.TryParse(workflowStateCorrelationId, out sessionId))
            return sessionId;
        return null;
    }
}

public sealed class KioskWorkflowExecutedHandler(KioskSagaService sagaService) : INotificationHandler<WorkflowExecuted>
{
    public async Task HandleAsync(WorkflowExecuted notification, CancellationToken cancellationToken)
    {
        Guid? sessionId = GetSessionId(notification.WorkflowExecutionContext.CorrelationId, notification.WorkflowState.CorrelationId);
        if (sessionId is null)
            return;

        if (notification.WorkflowState.SubStatus == WorkflowSubStatus.Cancelled)
        {
            await sagaService.HandleWorkflowCancelledAsync(sessionId.Value, cancellationToken);
            return;
        }

        if (notification.WorkflowState.SubStatus == WorkflowSubStatus.Faulted)
            await sagaService.HandleWorkflowFaultedAsync(sessionId.Value, cancellationToken);
    }

    private static Guid? GetSessionId(string? workflowExecutionContextCorrelationId, string? workflowStateCorrelationId)
    {
        if (Guid.TryParse(workflowExecutionContextCorrelationId, out Guid sessionId))
            return sessionId;
        if (Guid.TryParse(workflowStateCorrelationId, out sessionId))
            return sessionId;
        return null;
    }
}
