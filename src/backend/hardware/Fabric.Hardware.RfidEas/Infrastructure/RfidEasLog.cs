using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.RfidEas.Infrastructure;

internal static partial class RfidEasLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "RFID EAS reader starting")]
    public static partial void RfidReaderStarting(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "RFID EAS reader stopped")]
    public static partial void RfidReaderStopped(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Preparing to read card on RFID reader {ReaderId}")]
    public static partial void PreparingRead(this ILogger logger, int readerId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "RFID reader {ReaderId} already has a pending read")]
    public static partial void PendingReadAlreadyExists(this ILogger logger, int readerId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connecting to RFID EAS reader")]
    public static partial void Connecting(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Connected to RFID EAS reader")]
    public static partial void Connected(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "No RFID EAS reader found. Retrying after {TimeoutMs} ms")]
    public static partial void ConnectRetry(this ILogger logger, double timeoutMs);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{DeviceCount} RFID EAS reader(s) connected. Mapping logical IDs")]
    public static partial void MappingReaders(this ILogger logger, int deviceCount);

    [LoggerMessage(Level = LogLevel.Information, Message = "RFID EAS reader mapping done")]
    public static partial void MappingDone(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Read card on RFID reader {ReaderId} with card number {CardNumber}")]
    public static partial void CardRead(this ILogger logger, int readerId, string cardNumber);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Received {DataSize} bytes of RFID serial data")]
    public static partial void RfidSerialDataReceived(this ILogger logger, int dataSize);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Parsed RFID data to {CardNumber}")]
    public static partial void RfidCardParsed(this ILogger logger, string cardNumber);
}
