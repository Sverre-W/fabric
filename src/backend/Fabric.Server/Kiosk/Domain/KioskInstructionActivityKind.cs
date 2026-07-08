namespace Fabric.Server.Kiosk.Domain;

public enum KioskInstructionActivityKind
{
    Choice,
    Form
}

public static class KioskInstructionActivityKinds
{
    public static KioskInstructionActivityKind FromInstructionType(string instructionType) => instructionType switch
    {
        "prompt-choice" => KioskInstructionActivityKind.Choice,
        "display-form" => KioskInstructionActivityKind.Form,
        _ => throw new InvalidOperationException($"Unsupported kiosk instruction type '{instructionType}'.")
    };
}
