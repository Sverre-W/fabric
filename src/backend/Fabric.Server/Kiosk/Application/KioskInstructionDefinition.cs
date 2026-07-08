using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Kiosk.Application;

public sealed record KioskInstructionDefinition(
    KioskInstructionActivityKind Kind,
    string Type,
    KioskInstructionLayout Layout,
    KioskInstructionContent Content,
    IReadOnlyList<string> Choices,
    IReadOnlyList<KioskFormField> Fields);
