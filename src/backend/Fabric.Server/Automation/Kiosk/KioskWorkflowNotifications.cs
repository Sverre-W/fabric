using Elsa.Mediator.Contracts;
using Elsa.Workflows;
using Elsa.Workflows.Models;
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

        switch (notification.WorkflowState.SubStatus)
        {
            case WorkflowSubStatus.Faulted:
                await sagaService.HandleWorkflowFaultedAsync(sessionId.Value, GetIncidentMessage(notification.WorkflowState.Incidents.LastOrDefault()), cancellationToken);
                break;
            case WorkflowSubStatus.Cancelled:
                await sagaService.HandleWorkflowCancelledAsync(sessionId.Value, cancellationToken);
                break;
            default:
                await sagaService.HandleWorkflowFinishedAsync(sessionId.Value, cancellationToken);
                break;
        }
    }

    private static string? GetIncidentMessage(ActivityIncident? incident)
        => !string.IsNullOrWhiteSpace(incident?.Message)
            ? incident.Message.Trim()
            : incident?.Exception?.Message;

    private static Guid? GetSessionId(string? workflowExecutionContextCorrelationId, string? workflowStateCorrelationId)
    {
        if (Guid.TryParse(workflowExecutionContextCorrelationId, out Guid sessionId))
            return sessionId;
        if (Guid.TryParse(workflowStateCorrelationId, out sessionId))
            return sessionId;
        return null;
    }
}
