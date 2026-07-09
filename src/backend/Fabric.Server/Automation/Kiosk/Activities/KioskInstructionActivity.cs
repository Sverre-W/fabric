using Elsa.Extensions;
using Elsa.Workflows;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Sagas.Kiosk;

namespace Fabric.Server.Automation.Kiosk.Activities;

public abstract class KioskInstructionActivity<TResult> : Activity<TResult>
{
    protected abstract KioskInstructionDefinition BuildInstruction(ActivityExecutionContext context);

    protected abstract ValueTask HandleSubmissionAsync(ActivityExecutionContext context, KioskInstructionResult response, out string outcome);

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        KioskWorkflowAccessor accessor = context.GetRequiredService<KioskWorkflowAccessor>();
        KioskSagaService sagaService = context.GetRequiredService<KioskSagaService>();
        Guid sessionId = await accessor.GetRequiredSessionIdAsync(context, context.CancellationToken);
        KioskInstructionBookmark bookmark = await sagaService.ScheduleInstructionAsync(
            sessionId,
            context.WorkflowExecutionContext.Id,
            BuildInstruction(context),
            context.CancellationToken);

        context.CreateBookmark(bookmark, ResumeAsync, includeActivityInstanceId: false);
    }

    private async ValueTask ResumeAsync(ActivityExecutionContext context)
    {
        KioskWorkflowAccessor accessor = context.GetRequiredService<KioskWorkflowAccessor>();
        KioskInstructionService instructionService = context.GetRequiredService<KioskInstructionService>();
        Guid sessionId = await accessor.GetRequiredSessionIdAsync(context, context.CancellationToken);
        await instructionService.ClearCurrentInstructionAsync(sessionId, context.CancellationToken);

        if (context.TryGetWorkflowInput(KioskWorkflowContext.CancelledInputName, out bool cancelled) && cancelled)
        {
            return;
        }

        KioskInstructionResult response = context.GetWorkflowInput<KioskInstructionResult>(KioskWorkflowContext.InstructionResponseInputName);
        await HandleSubmissionAsync(context, response, out string outcome);
        await context.CompleteActivityWithOutcomesAsync(outcome);
    }
}
