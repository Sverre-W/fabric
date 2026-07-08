namespace Fabric.Server.Automation.Kiosk;

public sealed record KioskInstructionSubmission(string InstructionId, IReadOnlyDictionary<string, string> Values);
