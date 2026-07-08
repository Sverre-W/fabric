namespace Fabric.Server.Kiosk.Contracts;

public sealed record KioskInstructionEnvelope(
    string InstructionId,
    int Version,
    string Type,
    string LanguageCode,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content,
    IReadOnlyList<KioskChoiceOption> Choices,
    IReadOnlyList<KioskFormField> Fields);

public sealed record KioskInstructionLayout(string Mode, string? BackgroundAssetName, string? ImageAssetName);

public sealed record KioskInstructionContent(string? Title, string? Message);

public sealed record KioskChoiceOption(string Value, string Label);

public sealed record KioskFormField(string Name, string Label, string? Placeholder, bool IsRequired, bool IsMaskRequired);
