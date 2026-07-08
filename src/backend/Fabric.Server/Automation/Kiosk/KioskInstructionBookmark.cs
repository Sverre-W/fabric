using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Automation.Kiosk;

public sealed record KioskInstructionBookmark(Guid SessionId, string InstructionId, KioskInstructionActivityKind Kind);
