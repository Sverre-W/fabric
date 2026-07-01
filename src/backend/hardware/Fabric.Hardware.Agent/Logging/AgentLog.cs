namespace Fabric.Hardware.Agent;

internal static partial class AgentLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Worker {WorkerName} started")]
    public static partial void WorkerStarted(this ILogger logger, string workerName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Worker {WorkerName} stopped")]
    public static partial void WorkerStopped(this ILogger logger, string workerName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Heartbeat post failed")]
    public static partial void HeartbeatFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Inventory post failed")]
    public static partial void InventoryFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Command poll failed")]
    public static partial void CommandPollFailed(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Command stream connected")]
    public static partial void CommandStreamConnected(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Command stream disconnected")]
    public static partial void CommandStreamDisconnected(this ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Command {CommandId} was no longer claimable")]
    public static partial void CommandClaimLost(this ILogger logger, Guid commandId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "QR reader {DeviceId} unavailable while executing command {CommandId}")]
    public static partial void CommandDeviceUnavailable(this ILogger logger, Guid commandId, string deviceId, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "QR event post failed for device {DeviceId}")]
    public static partial void QrEventPostFailed(this ILogger logger, string deviceId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "QR reader {DeviceId} opened on {ComPort}")]
    public static partial void QrReaderOpened(this ILogger logger, string deviceId, string comPort);

    [LoggerMessage(Level = LogLevel.Warning, Message = "QR reader {DeviceId} open failed on {ComPort}")]
    public static partial void QrReaderOpenFailed(this ILogger logger, string deviceId, string comPort, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispenser {DeviceId} opened on {ComPort}")]
    public static partial void DispenserOpened(this ILogger logger, string deviceId, string comPort);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dispenser {DeviceId} open failed on {ComPort}")]
    public static partial void DispenserOpenFailed(this ILogger logger, string deviceId, string comPort, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dispenser {DeviceId} command {Capability} failed")]
    public static partial void DispenserCommandFailed(this ILogger logger, string deviceId, string capability);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dispenser {DeviceId} drop after read failure failed")]
    public static partial void DispenserDropAfterReadFailureFailed(this ILogger logger, string deviceId, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Card present read failed for dispenser {DeviceId} and RFID reader {RfidReaderDeviceId}")]
    public static partial void CardPresentReadFailed(this ILogger logger, string deviceId, string rfidReaderDeviceId, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RFID EAS reader {DeviceId} is unavailable")]
    public static partial void RfidEasUnavailable(this ILogger logger, string deviceId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Collector {DeviceId} opened on {ComPort}")]
    public static partial void CollectorOpened(this ILogger logger, string deviceId, string comPort);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Collector {DeviceId} open failed on {ComPort}")]
    public static partial void CollectorOpenFailed(this ILogger logger, string deviceId, string comPort, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Collector {DeviceId} command {Capability} failed")]
    public static partial void CollectorCommandFailed(this ILogger logger, string deviceId, string capability);
}
