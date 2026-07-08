using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Options;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Automation.Kiosk;

public sealed class KioskWorkflowResumer(IWorkflowResumer workflowResumer, KioskDbContext db)
{
    public async Task ResumeInstructionAsync(Guid sessionId, string instructionId, IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken)
    {
        KioskSession? session = await db.Sessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
        if (session is null)
            throw new InvalidOperationException($"Kiosk session '{sessionId}' not found.");

        var input = new Dictionary<string, object>
        {
            [KioskWorkflowContext.InstructionResponseInputName] = new KioskInstructionSubmission(instructionId, values)
        };
        var options = new ResumeBookmarkOptions { Input = input };

        var resumed = await workflowResumer.ResumeAsync<Activities.ShowChoiceInstruction>(new KioskInstructionBookmark(sessionId, instructionId), session.WorkflowInstanceId!, options, cancellationToken);

        if (!resumed.Any())
            resumed = await workflowResumer.ResumeAsync<Activities.ShowFormInstruction>(new KioskInstructionBookmark(sessionId, instructionId), session.WorkflowInstanceId!, options, cancellationToken);

        if (!resumed.Any())
            throw new InvalidOperationException($"No kiosk workflow was waiting for instruction '{instructionId}'.");
    }

    public async Task CancelSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        KioskSession? session = await db.Sessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
        if (session is null || string.IsNullOrWhiteSpace(session.CurrentInstructionId))
            return;

        var options = new ResumeBookmarkOptions { Input = new Dictionary<string, object> { [KioskWorkflowContext.CancelledInputName] = true } };
        var stimulus = new KioskInstructionBookmark(sessionId, session.CurrentInstructionId);

        var resumed = await workflowResumer.ResumeAsync<Activities.ShowChoiceInstruction>(stimulus, session.WorkflowInstanceId!, options, cancellationToken);

        if (!resumed.Any())
            await workflowResumer.ResumeAsync<Activities.ShowFormInstruction>(stimulus, session.WorkflowInstanceId!, options, cancellationToken);
    }
}
