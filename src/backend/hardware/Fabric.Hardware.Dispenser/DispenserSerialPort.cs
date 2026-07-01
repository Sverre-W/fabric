using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Dispenser;

public sealed class DispenserSerialPort : IDisposable
{
    private const byte StartFrame = 0x21; // '!'
    private const byte EndFrame = 0x0D;   // CR
    private const byte Ack = 0x06;

    private const byte AskStatus = 0x41;     // 'A'
    private const byte DispenseCard = 0x42;  // 'B'
    private const byte CaptureCard = 0x44;   // 'D'
    private const byte CapturePosition = 0x46; // 'F'
    private const byte FirmwareVersion = 0x56; // 'V'

    private readonly TimeSpan _responseTimeout;
    private readonly SerialPort _serialPort;
    private readonly ILogger<DispenserSerialPort> _logger;
    private readonly object _sync = new();

    private readonly List<byte> _receiveBuffer = new();
    private TaskCompletionSource<byte>? _pendingStatus;
    private bool _disposed;

    public DispenserSerialPort(ILogger<DispenserSerialPort> logger, string comPort, TimeSpan? responseTimeout = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(comPort))
            throw new ArgumentException("COM port must be specified.", nameof(comPort));

        TimeSpan timeout = responseTimeout ?? TimeSpan.FromSeconds(5);
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(responseTimeout));

        _responseTimeout = timeout;

        _serialPort = new SerialPort(comPort, 9600, Parity.None)
        {
            DataBits = 8,
            StopBits = StopBits.One
        };

        _serialPort.DataReceived += SerialPort_DataReceived;
        _serialPort.Open();

        _logger.SerialPortOpened(_serialPort.PortName);
    }

    public async Task<bool> Dispense(CancellationToken cancellationToken = default)
    {
        byte status = await ExecuteCommandForStatus(DispenseCard, cancellationToken).ConfigureAwait(false);
        LogStatus("Dispense", status);
        return status == 0x05 && !IsWrongCommand(status);
    }

    public async Task<bool> Drop(CancellationToken cancellationToken = default)
    {
        byte status = await ExecuteCommandForStatus(CaptureCard, cancellationToken).ConfigureAwait(false);
        LogStatus("Drop", status);
        return status == 0x06 && !IsWrongCommand(status);
    }

    public async Task<byte> GetStatus(CancellationToken cancellationToken = default)
    {
        byte status = await ExecuteCommandForStatus(AskStatus, cancellationToken).ConfigureAwait(false);
        LogStatus("Status", status);
        return status;
    }

    private void SerialPort_DataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (_disposed || !_serialPort.IsOpen)
                return;

            var length = _serialPort.BytesToRead;
            if (length <= 0)
                return;

            var data = new byte[length];
            _serialPort.Read(data, 0, length);

            _logger.DataReceived(BitConverter.ToString(data));

            List<byte> completedStatuses = new();

            lock (_sync)
            {
                _receiveBuffer.AddRange(data);

                while (TryExtractNextToken(_receiveBuffer, out var tokenType, out var value))
                {
                    if (tokenType == ReceivedTokenType.ImmediateAck)
                    {
                        _logger.ImmediateResponse(value);
                    }
                    else if (tokenType == ReceivedTokenType.StatusFrame)
                    {
                        completedStatuses.Add(value);
                    }
                }
            }

            foreach (var status in completedStatuses)
            {
                CompletePendingStatus(status);
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore dispose race.
        }
        catch (Exception ex)
        {
            _logger.SerialPortReadFailed(ex);
        }
    }

    private void CompletePendingStatus(byte status)
    {
        TaskCompletionSource<byte>? pending;

        lock (_sync)
        {
            pending = _pendingStatus;
            _pendingStatus = null;
        }

        if (pending is null)
        {
            _logger.UnsolicitedStatus(status);
            return;
        }

        pending.TrySetResult(status);
    }

    private async Task<byte> ExecuteCommandForStatus(byte command, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource<byte> pending;

        lock (_sync)
        {
            if (_pendingStatus is not null)
                throw new InvalidOperationException("Dispenser is busy.");

            pending = new TaskCompletionSource<byte>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingStatus = pending;
        }

        try
        {
            byte[] frame = [StartFrame, command, EndFrame];

            _logger.SendingCommand(BitConverter.ToString(frame));
            _serialPort.Write(frame, 0, frame.Length);
            _logger.CommandSent();

            return await pending.Task.WaitAsync(_responseTimeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            _logger.CommandTimedOut(command, _responseTimeout.TotalMilliseconds);

            lock (_sync)
            {
                if (ReferenceEquals(_pendingStatus, pending))
                    _pendingStatus = null;
            }

            return 0x00;
        }
        catch
        {
            lock (_sync)
            {
                if (ReferenceEquals(_pendingStatus, pending))
                    _pendingStatus = null;
            }

            throw;
        }
    }

    private enum ReceivedTokenType
    {
        ImmediateAck,
        StatusFrame
    }

    private static bool TryExtractNextToken(List<byte> buffer, out ReceivedTokenType tokenType, out byte value)
    {
        tokenType = default;
        value = 0;

        if (buffer.Count == 0)
            return false;

        // Case 1: immediate ack frame: 06 0D
        if (buffer.Count >= 2 && buffer[0] == Ack && buffer[1] == EndFrame)
        {
            tokenType = ReceivedTokenType.ImmediateAck;
            value = Ack;
            buffer.RemoveRange(0, 2);
            return true;
        }

        // Case 2: status frame: 21 xx 0D
        if (buffer[0] == StartFrame)
        {
            if (buffer.Count < 3)
                return false; // wait for more bytes

            if (buffer[2] == EndFrame)
            {
                tokenType = ReceivedTokenType.StatusFrame;
                value = buffer[1];
                buffer.RemoveRange(0, 3);
                return true;
            }

            // Badly aligned start byte, discard one byte and resync
            buffer.RemoveAt(0);
            return false;
        }

        // Unknown leading byte: discard and continue next time
        buffer.RemoveAt(0);
        return false;
    }

    private void LogStatus(string operation, byte status)
    {
        _logger.CommandStatus(
            operation,
            status,
            Convert.ToString(status, 2).PadLeft(8, '0'),
            (status >> 0) & 1,
            (status >> 1) & 1,
            (status >> 2) & 1,
            (status >> 3) & 1,
            (status >> 4) & 1,
            (status >> 5) & 1,
            (status >> 6) & 1,
            (status >> 7) & 1);
    }

    private static bool IsWrongCommand(byte status) => (status & 0x20) != 0;

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DispenserSerialPort));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _serialPort.DataReceived -= SerialPort_DataReceived;

        lock (_sync)
        {
            _pendingStatus?.TrySetCanceled();
            _pendingStatus = null;
            _receiveBuffer.Clear();
        }

        _serialPort.Dispose();
    }
}
