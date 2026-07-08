using Elsa.Workflows;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskWorkflowAccessor(KioskDbContext db)
{
    public async Task<KioskSession> GetRequiredSessionAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        string workflowInstanceId = GetWorkflowInstanceId(context);
        KioskSession? session = await db.Sessions.SingleOrDefaultAsync(x => x.WorkflowInstanceId == workflowInstanceId, cancellationToken);

        return session ?? throw new InvalidOperationException($"Kiosk session not found for workflow instance '{workflowInstanceId}'.");
    }

    public static string GetWorkflowInstanceId(ActivityExecutionContext context) => context.WorkflowExecutionContext.Id;
}
