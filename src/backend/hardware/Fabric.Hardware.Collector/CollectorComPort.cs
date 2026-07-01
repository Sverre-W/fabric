using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Collector;

public sealed class CollectorComPort : IDisposable
{
    private readonly TimeSpan _ackTimeout;
    private readonly TimeSpan _ackTimeoutMechanical;
    private readonly object _lock = new();
    private readonly ILogger<CollectorComPort> _logger;
    private readonly SerialPort _serialPort;

    private TaskCompletionSource<CollectorAcknowledge>? _acknowledgeStatus;
    private byte[] _accumulatedData = [];
    private bool _disposed;

    public Action<CollectorSensorState>? OnStateReceived { get; init; }

    public CollectorComPort(
        ILogger<CollectorComPort> logger,
        string comPort,
        TimeSpan? ackTimeout = null,
        TimeSpan? ackTimeoutMechanical = null)
    {
        _logger = logger;
        _ackTimeout = ackTimeout ?? TimeSpan.FromMilliseconds(500);
        _ackTimeoutMechanical = ackTimeoutMechanical ?? TimeSpan.FromSeconds(3);

        _serialPort = new SerialPort(comPort, 9600, Parity.None)
        {
            DataBits = 8,
            StopBits = StopBits.One,
        };
        _serialPort.Open();
        _serialPort.DataReceived += SerialPort_DataReceived;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _serialPort.DataReceived -= SerialPort_DataReceived;

        lock (_lock)
        {
            _acknowledgeStatus?.TrySetCanceled();
            _acknowledgeStatus = null;
            _accumulatedData = [];
        }

        _serialPort.Dispose();
    }

    public async Task<CollectorAcknowledge> ExecuteCommand(CollectorCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogCommandExecuting(command);

        TaskCompletionSource<CollectorAcknowledge> acknowledgeStatus = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_acknowledgeStatus is not null)
                throw new InvalidOperationException("Already processing a command.");

            _acknowledgeStatus = acknowledgeStatus;
        }

        try
        {
            byte[] frame = CreateCommandFrame(command);
            _serialPort.Write(frame, 0, frame.Length);

            TimeSpan waitTime = command switch
            {
                CollectorCommand.ReadStatus => _ackTimeout,
                CollectorCommand.ClearJam => _ackTimeout,
                _ => _ackTimeoutMechanical,
            };

            CollectorAcknowledge result = await acknowledgeStatus.Task.WaitAsync(waitTime, cancellationToken);
            LogCommandExecuted(command, result);
            return result;
        }
        catch (TimeoutException)
        {
            LogCommandTimedOut(command);
            return CollectorAcknowledge.None;
        }
        finally
        {
            lock (_lock)
            {
                if (ReferenceEquals(_acknowledgeStatus, acknowledgeStatus))
                    _acknowledgeStatus = null;
            }
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            byte[] buffer = new byte[_serialPort.BytesToRead];
            if (buffer.Length == 0)
                return;

            _serialPort.Read(buffer, 0, buffer.Length);

            _logger.CollectorDataReceived(BitConverter.ToString(buffer));
            lock (_lock)
            {
                _accumulatedData = [.. _accumulatedData, .. buffer];
                ProcessData();
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void ProcessData()
    {
        while (_accumulatedData.Length > 0)
        {
            if (IsAcknowledge(_accumulatedData[0], out CollectorAcknowledge? acknowledge))
            {
                _acknowledgeStatus?.TrySetResult(acknowledge!.Value);
                _accumulatedData = _accumulatedData[1..];
                continue;
            }

            if (_accumulatedData[0] == (byte)CollectorControlCharacters.Stx)
            {
                int endOfCommand = Array.IndexOf(_accumulatedData, (byte)CollectorControlCharacters.Etx);
                int bccIndex = endOfCommand + 1;

                if (endOfCommand < 0 || _accumulatedData.Length <= bccIndex)
                    return;

                byte[] commandData = _accumulatedData[1..endOfCommand];
                _accumulatedData = _accumulatedData[(bccIndex + 1)..];

                var state = new CollectorSensorState(commandData);
                OnStateReceived?.Invoke(state);
                continue;
            }

            _accumulatedData = _accumulatedData[1..];
        }
    }

    private static byte[] CreateCommandFrame(CollectorCommand command)
    {
        byte[] frame = [
            (byte)CollectorControlCharacters.Stx,
            (byte)command,
            (byte)CollectorControlCharacters.Etx,
            0x00
        ];
        frame[3] = frame.Aggregate((byte)0x00, (current, value) => (byte)(current ^ value));
        return frame;
    }

    private void LogCommandExecuting(CollectorCommand command)
    {
        if (command == CollectorCommand.ReadStatus)
            _logger.CollectorStatusCommandExecuting(command);
        else
            _logger.CollectorCommandExecuting(command);
    }

    private void LogCommandExecuted(CollectorCommand command, CollectorAcknowledge acknowledge)
    {
        if (command == CollectorCommand.ReadStatus)
            _logger.CollectorStatusCommandExecuted(command, acknowledge);
        else
            _logger.CollectorCommandExecuted(command, acknowledge);
    }

    private void LogCommandTimedOut(CollectorCommand command)
    {
        if (command == CollectorCommand.ReadStatus)
            _logger.CollectorStatusCommandTimedOut(command);
        else
            _logger.CollectorCommandTimedOut(command);
    }

    private static bool IsAcknowledge(byte data, out CollectorAcknowledge? acknowledge)
    {
        int dataValue = data;
        if (Enum.IsDefined(typeof(CollectorAcknowledge), dataValue))
        {
            acknowledge = (CollectorAcknowledge)dataValue;
            return true;
        }

        acknowledge = null;
        return false;
    }
}

public enum CollectorControlCharacters
{
    Stx = 0x02,
    Etx = 0x03,
    Enq = 0x05,
}

public enum CollectorAcknowledge
{
    None = 0x00,
    Ack = 0x06,
    Nack = 0x15,
    Can = 0x18,
}

public enum CollectorCommand
{
    ClearJam = 0x30,
    ReadStatus = 0x31,
    CollectCard = 0x40,
    CaptureCard = 0x45,
    FeedHold = 0x47,
    FeedStandBy = 0x48,
}
