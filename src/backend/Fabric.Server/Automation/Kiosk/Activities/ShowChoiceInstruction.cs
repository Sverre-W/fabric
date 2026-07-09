using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Attributes;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Automation.Kiosk.Activities;

[Activity("Fabric", "Kiosk", "Show a choice instruction and wait for kiosk input.", DisplayName = "Show Choice Instruction")]
[FlowNode("Done", "Cancelled")]
public sealed class ShowChoiceInstruction : KioskInstructionActivity<string>
{
    [Input(DisplayName = "Layout mode", Description = "default, split-left-visual, or split-right-visual")]
    public Input<string> Mode { get; set; } = new("default");

    [Input(DisplayName = "Background asset")]
    public Input<string?> BackgroundAssetName { get; set; } = default!;

    [Input(DisplayName = "Image asset")]
    public Input<string?> ImageAssetName { get; set; } = default!;

    [Input(DisplayName = "Title")]
    public Input<string?> Title { get; set; } = default!;

    [Input(DisplayName = "Message")]
    public Input<string?> Message { get; set; } = default!;

    [Input(DisplayName = "Choices", Description = "Array of { value, label }.", UIHint = InputUIHints.DynamicOutcomes)]
    public Input<ICollection<string>> Choices { get; set; } = default!;

    protected override KioskInstructionDefinition BuildInstruction(ActivityExecutionContext context)
    {
        return new KioskInstructionDefinition(
            KioskInstructionActivityKind.Choice,
            "prompt-choice",
            new KioskInstructionLayout(context.Get(Mode) ?? "default", context.Get(BackgroundAssetName), context.Get(ImageAssetName)),
            new KioskInstructionContent(context.Get(Title), context.Get(Message)),
            [.. context.Get(Choices) ?? []],
            []);
    }

    protected override ValueTask HandleSubmissionAsync(ActivityExecutionContext context, KioskInstructionResult response, out string outcome)
    {
        string value = response is KioskChoiceInstructionResult choiceResult
            ? choiceResult.Value
            : throw new InvalidOperationException($"Expected {nameof(KioskChoiceInstructionResult)} but received {response.GetType().Name}.");
        context.Set(Result, value);
        outcome = value;
        return ValueTask.CompletedTask;
    }
}
