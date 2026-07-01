using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Dispenser;

internal static partial class DispenserLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Opened dispenser serial port {Port}")]
    public static partial void SerialPortOpened(this ILogger logger, string port);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dispenser data received: {Data}")]
    public static partial void DataReceived(this ILogger logger, string data);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dispenser immediate response: 0x{Ack:X2}")]
    public static partial void ImmediateResponse(this ILogger logger, byte ack);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Sending dispenser command: {Data}")]
    public static partial void SendingCommand(this ILogger logger, string data);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Dispenser command sent")]
    public static partial void CommandSent(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "{Operation} status: 0x{Status:X2} ({Bits}) bit0={Bit0}, bit1={Bit1}, bit2={Bit2}, bit3={Bit3}, bit4={Bit4}, bit5={Bit5}, bit6={Bit6}, bit7={Bit7}")]
    public static partial void CommandStatus(this ILogger logger, string operation, byte status, string bits, int bit0, int bit1, int bit2, int bit3, int bit4, int bit5, int bit6, int bit7);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Received dispenser status 0x{Status:X2}, but no command is waiting for it")]
    public static partial void UnsolicitedStatus(this ILogger logger, byte status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dispenser command 0x{Command:X2} timed out after {TimeoutMs} ms")]
    public static partial void CommandTimedOut(this ILogger logger, byte command, double timeoutMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error while reading dispenser serial port data")]
    public static partial void SerialPortReadFailed(this ILogger logger, Exception exception);
}
