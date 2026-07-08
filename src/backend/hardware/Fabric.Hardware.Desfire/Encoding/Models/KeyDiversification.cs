namespace Fabric.Hardware.Desfire.Encoding.Models;

public enum KeyDiversificationAlgorithm
{
    /// <summary>
    ///     No diversification algorithm (default value, should not be used)
    /// </summary>
    None,

    /// <summary>
    ///     Diversification algorithm as described in AN10922 (CMAC)
    /// </summary>
    NxpAn10922,
}

public enum DiversificationInputOptions
{
    /// <summary>
    ///     The UID of the card (7 bytes)
    /// </summary>
    Uid,

    /// <summary>
    ///     A 4 byte Uid  of the card
    /// </summary>
    Uid4Bytes,

    /// <summary>
    ///     The current application id, if PICC key 000000 is  used
    /// </summary>
    ApplicationId,

    /// <summary>
    ///     The reversed application id (LSB to MSB)
    /// </summary>
    ApplicationIdReversed,

    /// <summary>
    ///     The KeyNo of the key in the key set ranging from 0 to 14
    /// </summary>
    KeyNo,

    /// <summary>
    ///     A fixed hex value
    /// </summary>
    FixedHexValue,
}

/// <summary>
///     Describes the strategy of how to diversify keys in a key group
/// </summary>
public class KeyDiversificationStrategy
{
    /// <summary>
    ///     The unique identifier of the strategy
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The name of the strategy
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The algorithm used diversify the key and the diversification input
    /// </summary>
    public KeyDiversificationAlgorithm Algorithm { get; set; } = KeyDiversificationAlgorithm.None;

    /// <summary>
    ///     A list of ordered input options to form the diversification input with a max length of (31
    ///     bytes)
    /// </summary>
    public List<DiversificationInput> Inputs { get; set; } = [];
}

public class DiversificationInput
{
    public DiversificationInputOptions Option { get; set; }
    public string? Data { get; set; } = null!;

    public int LengthInBytes =>
        Option switch
        {
            DiversificationInputOptions.Uid => 7,
            DiversificationInputOptions.Uid4Bytes => 4,
            DiversificationInputOptions.ApplicationId => 3,
            DiversificationInputOptions.ApplicationIdReversed => 3,
            DiversificationInputOptions.KeyNo => 1,
            DiversificationInputOptions.FixedHexValue => Data!.Length * 2,
            _ => throw new ArgumentOutOfRangeException(),
        };
}
