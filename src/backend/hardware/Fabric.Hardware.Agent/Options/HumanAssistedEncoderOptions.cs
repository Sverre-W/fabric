namespace Fabric.Hardware.Agent.Options;

public sealed class HumanAssistedEncoderOptions : EncoderOptions
{
    public required string Reader { get; init; }

    public PcscEncoderImplementation Implementation { get; init; } = PcscEncoderImplementation.Iso;
}
