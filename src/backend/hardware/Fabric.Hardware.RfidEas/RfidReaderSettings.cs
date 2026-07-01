namespace Fabric.Hardware.RfidEas;

public sealed class RfidReaderSettings
{
    /// <summary>
    ///     The card format that should be read.
    /// </summary>
    public CardDataFormat CardFormat { get; init; } = CardDataFormat.BCD;

    /// <summary>
    ///     Applies a transformation to the given data before reading the data
    /// </summary>
    public CardDataTransformer Transformer { get; init; } = CardDataTransformer.None;

    /// <summary>
    /// A delay in milliseconds after a successful read before the next polling is done
    ///
    public TimeSpan DelayAfterRead { get; init; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// A delay in milliseconds between polling attempts
    /// </summary>
    public TimeSpan PollingDelay { get; init; } = TimeSpan.FromMilliseconds(250);
}

public enum CardDataFormat
{
    /// <summary>
    ///     Describes an unrecognized format
    /// </summary>
    Unknown,

    /// <summary>
    ///     The BCD card format
    /// </summary>
    BCD,

    /// <summary>
    ///     Nedap format
    /// </summary>
    Hexadecimal,
}

public enum CardDataTransformer
{
    None,
    InvertBits,
    InvertBytes,
}
