using Elsa.Extensions;
using Elsa.Workflows;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskWorkflowAccessor(KioskDbContext db)
{
    public async Task<Guid> GetRequiredSessionIdAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        Guid sessionId = GetRequiredSessionId(context);
        bool exists = await db.Sessions.AnyAsync(x => x.Id == sessionId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Kiosk session '{sessionId}' not found.");

        return sessionId;
    }

    public async Task<KioskSession> GetRequiredSessionAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        Guid sessionId = GetRequiredSessionId(context);
        KioskSession? session = await db.Sessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
        return session ?? throw new InvalidOperationException($"Kiosk session '{sessionId}' not found.");
    }

    public static Guid GetRequiredSessionId(ActivityExecutionContext context)
    {
        Guid sessionId = context.WorkflowExecutionContext.GetProperty<Guid>(KioskWorkflowContext.SessionIdPropertyName);
        if (sessionId != Guid.Empty)
            return sessionId;

        if (context.TryGetWorkflowInput<Guid>(KioskWorkflowContext.SessionIdInputName, out sessionId))
        {
            context.WorkflowExecutionContext.SetProperty(KioskWorkflowContext.SessionIdPropertyName, sessionId);
            return sessionId;
        }

        throw new InvalidOperationException("Kiosk session ID not found in workflow execution context.");
    }

    public static string GetWorkflowInstanceId(ActivityExecutionContext context) => context.WorkflowExecutionContext.Id;
}
