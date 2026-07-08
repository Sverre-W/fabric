namespace Fabric.Hardware.Desfire.Protocol;

/// <summary>
///     The communication mode with the card
/// </summary>
/// <remarks>DS487033 Page 33</remarks>
public enum CommunicationMode
{
    /// <summary>
    ///     Plain communication, no encryption is used
    /// </summary>
    Plain,

    /// <summary>
    ///     MACed communication, data is transferred in plain text, but a 4 or 8 byte MAC is added to the message
    /// </summary>
    Cmac,

    /// <summary>
    ///     Encrypted communication, data is encrypted using the given <see cref="KeyType" />. CRC is added (CRC16 or CRC32)
    /// </summary>
    Enciphered,
}
