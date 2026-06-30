namespace Fabric.Hardware.Contracts.Commands;

public sealed record HardwareCommandClaimResponse(
    HardwareCommandEnvelope Command,
    DateTimeOffset LeaseExpiresAt);
