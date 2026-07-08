using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Options;
using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskWorkflowResumer(IWorkflowResumer workflowResumer)
{
    public async Task ResumeInstructionAsync(KioskSession session, string instructionId, KioskInstructionActivityKind kind, KioskInstructionResult result, CancellationToken cancellationToken)
    {
        var input = new Dictionary<string, object>
        {
            [KioskWorkflowContext.InstructionResponseInputName] = result
        };
        var options = new ResumeBookmarkOptions { Input = input };

        var resumed = await ResumeAsync(kind, new KioskInstructionBookmark(session.Id, instructionId, kind), session.WorkflowInstanceId!, options, cancellationToken);

        if (!resumed.Any())
            throw new InvalidOperationException($"No kiosk workflow was waiting for instruction '{instructionId}'.");
    }

    public async Task CancelInstructionAsync(KioskSession session, string instructionId, KioskInstructionActivityKind kind, CancellationToken cancellationToken)
    {
        var options = new ResumeBookmarkOptions { Input = new Dictionary<string, object> { [KioskWorkflowContext.CancelledInputName] = true } };
        var stimulus = new KioskInstructionBookmark(session.Id, instructionId, kind);
        await ResumeAsync(kind, stimulus, session.WorkflowInstanceId!, options, cancellationToken);
    }

    private async Task<IEnumerable<object>> ResumeAsync(KioskInstructionActivityKind kind, KioskInstructionBookmark stimulus, string workflowInstanceId, ResumeBookmarkOptions options, CancellationToken cancellationToken) => kind switch
    {
        KioskInstructionActivityKind.Choice => await workflowResumer.ResumeAsync<Activities.ShowChoiceInstruction>(stimulus, workflowInstanceId, options, cancellationToken),
        KioskInstructionActivityKind.Form => await workflowResumer.ResumeAsync<Activities.ShowFormInstruction>(stimulus, workflowInstanceId, options, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported kiosk instruction kind '{kind}'.")
    };
}
