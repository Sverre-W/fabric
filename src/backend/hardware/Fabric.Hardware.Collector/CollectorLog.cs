using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Collector;

internal static partial class CollectorLog
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Received collector data: {Data}")]
    public static partial void CollectorDataReceived(this ILogger logger, string data);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Executing collector status command {Command}")]
    public static partial void CollectorStatusCommandExecuting(this ILogger logger, CollectorCommand command);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Executing collector command {Command}")]
    public static partial void CollectorCommandExecuting(this ILogger logger, CollectorCommand command);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Executed collector status command {Command}: {Acknowledge}")]
    public static partial void CollectorStatusCommandExecuted(this ILogger logger, CollectorCommand command, CollectorAcknowledge acknowledge);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Executed collector command {Command}: {Acknowledge}")]
    public static partial void CollectorCommandExecuted(this ILogger logger, CollectorCommand command, CollectorAcknowledge acknowledge);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Collector status command {Command} timed out")]
    public static partial void CollectorStatusCommandTimedOut(this ILogger logger, CollectorCommand command);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Collector command {Command} timed out")]
    public static partial void CollectorCommandTimedOut(this ILogger logger, CollectorCommand command);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Collector command {Command} failed with {Acknowledge}")]
    public static partial void CollectorCommandFailed(this ILogger logger, CollectorCommand command, CollectorAcknowledge acknowledge);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Collector state changed: {StateChange}")]
    public static partial void CollectorStateChanged(this ILogger logger, StateTransition stateChange);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Sensor state indicates collector is busy")]
    public static partial void CollectorBusy(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Collector max jam retries reached")]
    public static partial void CollectorMaxJamRetriesReached(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Collector stack is full")]
    public static partial void CollectorStackFull(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Two cards in collector")]
    public static partial void CollectorTwoCardsDetected(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Collector card detected")]
    public static partial void CollectorCardDetected(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Collector card in front of reader")]
    public static partial void CollectorCardAtReader(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Collector state indicates jam, trying to clear")]
    public static partial void CollectorClearingJam(this ILogger logger);
}
