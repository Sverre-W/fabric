using Elsa.Extensions;
using Elsa.Workflows;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Sagas.Kiosk;

namespace Fabric.Server.Automation.Kiosk.Activities;

public abstract class KioskInstructionActivity<TResult> : Activity<TResult>
{
    protected abstract KioskInstructionDefinition BuildInstruction(ActivityExecutionContext context);

    protected abstract ValueTask HandleSubmissionAsync(ActivityExecutionContext context, KioskInstructionResult response);

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var sagaService = context.GetRequiredService<KioskSagaService>();
        KioskInstructionBookmark bookmark = await sagaService.ScheduleInstructionAsync(
            context.WorkflowExecutionContext.Id,
            BuildInstruction(context),
            context.CancellationToken);

        context.CreateBookmark(bookmark, ResumeAsync, includeActivityInstanceId: false);
    }

    private async ValueTask ResumeAsync(ActivityExecutionContext context)
    {
        var instructionService = context.GetRequiredService<KioskInstructionService>();
        await instructionService.ClearCurrentInstructionAsync(context.WorkflowExecutionContext.Id, context.CancellationToken);

        if (context.TryGetWorkflowInput<bool>(KioskWorkflowContext.CancelledInputName, out bool cancelled) && cancelled)
        {
            await context.CompleteActivityWithOutcomesAsync("Cancelled");
            return;
        }

        KioskInstructionResult response = context.GetWorkflowInput<KioskInstructionResult>(KioskWorkflowContext.InstructionResponseInputName);
        await HandleSubmissionAsync(context, response);
        await context.CompleteActivityWithOutcomesAsync("Done");
    }
}
