using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;

namespace Fabric.Server.Automation.Kiosk.Activities;

[Activity("Fabric", "Kiosk", "Mark current kiosk session as cancelled.", DisplayName = "Cancel Kiosk Session")]
public sealed class CancelKioskSession : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var accessor = context.GetRequiredService<KioskWorkflowAccessor>();
        var db = context.GetRequiredService<Fabric.Server.Kiosk.Persistence.KioskDbContext>();
        var timeProvider = context.GetRequiredService<TimeProvider>();
        var session = await accessor.GetRequiredSessionAsync(context, context.CancellationToken);
        session.Cancel(timeProvider.GetUtcNow());
        session.ClearInstruction(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
