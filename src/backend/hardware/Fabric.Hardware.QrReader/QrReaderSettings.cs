namespace Fabric.Hardware.QrReader;

/// <summary>
/// The settings for the QR reader.
/// </summary>
public sealed class QrReaderSettings
{
    /// <summary>
    /// The COM port used by the QR reader.
    /// </summary>
    public required string ComPort { get; init; }
}
