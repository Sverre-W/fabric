using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Attributes;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Fabric.Server.Kiosk.Contracts;

namespace Fabric.Server.Automation.Kiosk.Activities;

[Activity("Fabric", "Kiosk", "Show a choice instruction and wait for kiosk input.", DisplayName = "Show Choice Instruction")]
[FlowNode("Done", "Cancelled")]
public sealed class ShowChoiceInstruction : Activity<string>
{
    [Input(Description = "Instruction type.")]
    public Input<string> InstructionType { get; set; } = new("choice");

    [Input(Description = "Layout mode.")]
    public Input<string> Mode { get; set; } = new("default");

    [Input]
    public Input<string?> BackgroundUrl { get; set; } = default!;

    [Input]
    public Input<string?> ImageUrl { get; set; } = default!;

    [Input]
    public Input<string?> Title { get; set; } = default!;

    [Input]
    public Input<string?> TitleKey { get; set; } = default!;

    [Input]
    public Input<string?> Message { get; set; } = default!;

    [Input]
    public Input<string?> MessageKey { get; set; } = default!;

    [Input]
    public Input<IDictionary<string, string>?> Theme { get; set; } = default!;

    [Input]
    public Input<ICollection<KioskChoiceOption>?> Choices { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var accessor = context.GetRequiredService<KioskWorkflowAccessor>();
        var writer = context.GetRequiredService<KioskInstructionWriter>();
        var session = await accessor.GetRequiredSessionAsync(context, context.CancellationToken);
        KioskInstructionBookmark bookmark = await writer.WriteInstructionAsync(
            context,
            session,
            context.Get(InstructionType) ?? "choice",
            context.Get(Mode) ?? "default",
            context.Get(BackgroundUrl),
            context.Get(ImageUrl),
            context.Get(Title),
            context.Get(TitleKey),
            context.Get(Message),
            context.Get(MessageKey),
            context.Get(Theme) is { } theme ? new Dictionary<string, string>(theme) : null,
            context.Get(Choices)?.ToArray(),
            null,
            context.CancellationToken);

        context.CreateBookmark(bookmark, ResumeAsync, includeActivityInstanceId: false);
    }

    private async ValueTask ResumeAsync(ActivityExecutionContext context)
    {
        var accessor = context.GetRequiredService<KioskWorkflowAccessor>();
        var db = context.GetRequiredService<Fabric.Server.Kiosk.Persistence.KioskDbContext>();
        var timeProvider = context.GetRequiredService<TimeProvider>();
        var session = await accessor.GetRequiredSessionAsync(context, context.CancellationToken);
        session.ClearInstruction(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(context.CancellationToken);

        if (context.TryGetWorkflowInput<bool>(KioskWorkflowContext.CancelledInputName, out bool cancelled) && cancelled)
        {
            await context.CompleteActivityWithOutcomesAsync("Cancelled");
            return;
        }

        KioskInstructionSubmission response = context.GetWorkflowInput<KioskInstructionSubmission>(KioskWorkflowContext.InstructionResponseInputName);
        context.Set(Result, response.Values.GetValueOrDefault("value") ?? string.Empty);
        await context.CompleteActivityWithOutcomesAsync("Done");
    }
}
