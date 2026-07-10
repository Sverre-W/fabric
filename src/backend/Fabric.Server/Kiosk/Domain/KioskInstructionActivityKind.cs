namespace Fabric.Server.Kiosk.Domain;

public enum KioskInstructionActivityKind
{
    Message,
    Choice,
    Form
}

public static class KioskInstructionActivityKinds
{
    public static KioskInstructionActivityKind FromInstructionType(string instructionType) => instructionType switch
    {
        "display-message" => KioskInstructionActivityKind.Message,
        "prompt-choice" => KioskInstructionActivityKind.Choice,
        "display-form" => KioskInstructionActivityKind.Form,
        _ => throw new InvalidOperationException($"Unsupported kiosk instruction type '{instructionType}'.")
    };
}
