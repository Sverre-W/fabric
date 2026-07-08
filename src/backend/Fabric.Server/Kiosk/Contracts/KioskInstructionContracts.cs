namespace Fabric.Server.Kiosk.Contracts;

public sealed record KioskInstructionEnvelope(
    string InstructionId,
    int Version,
    string Type,
    string LanguageCode,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content,
    IReadOnlyDictionary<string, string> Theme,
    IReadOnlyList<KioskChoiceOption> Choices,
    IReadOnlyList<KioskFormField> Fields);

public sealed record KioskInstructionLayout(string Mode, string? BackgroundUrl, string? ImageUrl);

public sealed record KioskInstructionContent(string? Title, string? TitleKey, string? Message, string? MessageKey);

public sealed record KioskChoiceOption(string Value, string Label, string? LabelKey);

public sealed record KioskFormField(string Name, string? Label, string? LabelKey, string? Placeholder, string? PlaceholderKey, bool IsRequired, bool IsMaskRequired);
