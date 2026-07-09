using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Attributes;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Automation.Kiosk.Activities;

[Activity("Fabric", "Kiosk", "Show a form instruction and wait for kiosk input.", DisplayName = "Show Form Instruction")]
[FlowNode("Done")]
public sealed class ShowFormInstruction : KioskInstructionActivity<IDictionary<string, string>>
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

    [Input(DisplayName = "Fields", Description = "Array of { name, label, placeholder, isRequired, isMaskRequired }.")]
    public Input<ICollection<KioskFormField>?> Fields { get; set; } = default!;

    protected override KioskInstructionDefinition BuildInstruction(ActivityExecutionContext context)
    {
        return new KioskInstructionDefinition(
            KioskInstructionActivityKind.Form,
            "display-form",
            new KioskInstructionLayout(context.Get(Mode) ?? "default", context.Get(BackgroundAssetName), context.Get(ImageAssetName)),
            new KioskInstructionContent(context.Get(Title), context.Get(Message)),
            [],
            [.. context.Get(Fields) ?? []]);
    }

    protected override ValueTask HandleSubmissionAsync(ActivityExecutionContext context, KioskInstructionResult response, out string outcome)
    {
        IReadOnlyDictionary<string, string> values = response is KioskFormInstructionResult formResult
            ? formResult.Values
            : throw new InvalidOperationException($"Expected {nameof(KioskFormInstructionResult)} but received {response.GetType().Name}.");
        context.Set(Result, new Dictionary<string, string>(values));
        outcome = "Done";
        return ValueTask.CompletedTask;
    }
}
