namespace Fabric.Hardware.Agent.Options;

public sealed class DispenserEncoderOptions : EncoderOptions
{
    public required string ComPort { get; init; }

    public required string Reader { get; init; }

    public PcscEncoderImplementation Implementation { get; init; } = PcscEncoderImplementation.Iso;

    public TimeSpan ResponseTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
