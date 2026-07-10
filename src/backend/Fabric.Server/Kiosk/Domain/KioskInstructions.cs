using System.Text.Json;
using Fabric.Server.Kiosk;

namespace Fabric.Server.Kiosk.Domain;

public abstract record KioskInstruction(
    string InstructionId,
    int Version,
    string Type,
    string LanguageCode,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content)
{
    public abstract KioskInstructionActivityKind Kind { get; }

    public abstract KioskInstructionResult CreateResult(IReadOnlyDictionary<string, string> values);
}

public sealed record KioskMessageInstruction(
    string InstructionId,
    int Version,
    string Type,
    string LanguageCode,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content)
    : KioskInstruction(InstructionId, Version, Type, LanguageCode, Layout, Content)
{
    public override KioskInstructionActivityKind Kind => KioskInstructionActivityKind.Message;

    public override KioskInstructionResult CreateResult(IReadOnlyDictionary<string, string> values) =>
        throw new InvalidOperationException("Message instructions do not accept responses.");
}

public sealed record KioskChoiceInstruction(
    string InstructionId,
    int Version,
    string Type,
    string LanguageCode,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content,
    IReadOnlyList<KioskChoiceOption> Choices)
    : KioskInstruction(InstructionId, Version, Type, LanguageCode, Layout, Content)
{
    public override KioskInstructionActivityKind Kind => KioskInstructionActivityKind.Choice;

    public override KioskInstructionResult CreateResult(IReadOnlyDictionary<string, string> values) =>
        new KioskChoiceInstructionResult(InstructionId, values.GetValueOrDefault("value") ?? string.Empty);
}

public sealed record KioskFormInstruction(
    string InstructionId,
    int Version,
    string Type,
    string LanguageCode,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content,
    IReadOnlyList<KioskFormField> Fields)
    : KioskInstruction(InstructionId, Version, Type, LanguageCode, Layout, Content)
{
    public override KioskInstructionActivityKind Kind => KioskInstructionActivityKind.Form;

    public override KioskInstructionResult CreateResult(IReadOnlyDictionary<string, string> values) =>
        new KioskFormInstructionResult(InstructionId, new Dictionary<string, string>(values));
}

public sealed record KioskInstructionLayout(string Mode, string? BackgroundAssetName, string? ImageAssetName);

public sealed record KioskInstructionContent(string? Title, string? Message);

public sealed record KioskChoiceOption(string Value, string Label);

public sealed record KioskFormField(string Name, string Label, string? Placeholder, bool IsRequired, bool IsMaskRequired);

public abstract record KioskInstructionResult(string InstructionId);

public sealed record KioskChoiceInstructionResult(string InstructionId, string Value) : KioskInstructionResult(InstructionId);

public sealed record KioskFormInstructionResult(string InstructionId, IReadOnlyDictionary<string, string> Values) : KioskInstructionResult(InstructionId);

public static class KioskInstructionJsonSerializer
{
    public static string Serialize(KioskInstruction instruction) => instruction switch
    {
        KioskMessageInstruction message => JsonSerializer.Serialize(message, KioskJsonSerializerContext.Default.KioskMessageInstruction),
        KioskChoiceInstruction choice => JsonSerializer.Serialize(choice, KioskJsonSerializerContext.Default.KioskChoiceInstruction),
        KioskFormInstruction form => JsonSerializer.Serialize(form, KioskJsonSerializerContext.Default.KioskFormInstruction),
        _ => throw new InvalidOperationException($"Unsupported kiosk instruction type '{instruction.GetType().Name}'.")
    };

    public static KioskInstruction Deserialize(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        string type = document.RootElement.GetProperty("type").GetString() ?? throw new InvalidOperationException("Instruction type is missing.");

        return type switch
        {
            "display-message" => JsonSerializer.Deserialize(json, KioskJsonSerializerContext.Default.KioskMessageInstruction) ?? throw new InvalidOperationException("Message instruction is invalid."),
            "prompt-choice" => JsonSerializer.Deserialize(json, KioskJsonSerializerContext.Default.KioskChoiceInstruction) ?? throw new InvalidOperationException("Choice instruction is invalid."),
            "display-form" => JsonSerializer.Deserialize(json, KioskJsonSerializerContext.Default.KioskFormInstruction) ?? throw new InvalidOperationException("Form instruction is invalid."),
            _ => throw new InvalidOperationException($"Unsupported kiosk instruction type '{type}'.")
        };
    }
}
