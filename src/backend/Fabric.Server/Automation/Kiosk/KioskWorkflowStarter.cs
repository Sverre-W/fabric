using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Messages;
using Elsa.Workflows.Models;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskWorkflowStarter(IWorkflowRuntime workflowRuntime, KioskDbContext db, TimeProvider timeProvider)
{
    public async Task StartSessionWorkflowAsync(Fabric.Server.Kiosk.Domain.Kiosk kiosk, KioskSession session, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(kiosk.WorkflowDefinitionId))
            throw new InvalidOperationException("Kiosk has no assigned workflow definition.");

        var client = await workflowRuntime.CreateClientAsync(cancellationToken);
        var result = await client.CreateAndRunInstanceAsync(new CreateAndRunWorkflowInstanceRequest
        {
            WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(kiosk.WorkflowDefinitionId),
            CorrelationId = session.Id.ToString("N"),
            Input = new Dictionary<string, object>
            {
                [KioskWorkflowContext.KioskIdInputName] = kiosk.Id,
                [KioskWorkflowContext.SessionIdInputName] = session.Id,
                [KioskWorkflowContext.ProfileIdInputName] = kiosk.ProfileId,
                [KioskWorkflowContext.LanguageCodeInputName] = session.LanguageCode
            }
        });

        session.AssignWorkflowInstance(result.WorkflowInstanceId, timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }
}
