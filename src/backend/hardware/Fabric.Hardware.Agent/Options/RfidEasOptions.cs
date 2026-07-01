using Fabric.Hardware.RfidEas;

namespace Fabric.Hardware.Agent.Options;

public sealed class RfidEasOptions
{
    public CardDataFormat CardFormat { get; init; } = CardDataFormat.BCD;

    public CardDataTransformer Transformer { get; init; } = CardDataTransformer.None;

    public TimeSpan DelayAfterRead { get; init; } = TimeSpan.FromMilliseconds(250);

    public TimeSpan PollingDelay { get; init; } = TimeSpan.FromMilliseconds(250);

    public RfidEasReaderOptions[] Readers { get; init; } = [];
}
