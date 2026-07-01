using System.IO.Ports;
using System.Text;
using System.Threading.Channels;

namespace Fabric.Hardware.QrReader;

public sealed class QrReader : IDisposable
{
    private readonly object _gate = new();
    private readonly SerialPort _serialPort;
    private readonly Channel<string> _readings = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        AllowSynchronousContinuations = false,
        SingleReader = false,
        SingleWriter = false
    });
    private readonly StringBuilder _buffer = new();
    private TaskCompletionSource<string>? _pendingRead;
    private CancellationTokenRegistration _pendingReadCancellation;
    private bool _disposed;

    public QrReader(string comPort)
    {
        _serialPort = new SerialPort(comPort, 9600, Parity.None)
        {
            DataBits = 8,
            Encoding = Encoding.UTF8,
            StopBits = StopBits.One
        };

        _serialPort.Open();
        _serialPort.DataReceived += SerialPort_DataReceived;
    }

    public void Dispose()
    {
        TaskCompletionSource<string>? pendingRead;
        CancellationTokenRegistration pendingReadCancellation;

        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            pendingRead = _pendingRead;
            pendingReadCancellation = _pendingReadCancellation;
            _pendingRead = null;
            _pendingReadCancellation = default;
        }

        pendingReadCancellation.Dispose();
        pendingRead?.TrySetCanceled();
        _readings.Writer.TryComplete();
        _serialPort.DataReceived -= SerialPort_DataReceived;
        _serialPort.Dispose();
    }

    public Task<string> ReadQrCode(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        TaskCompletionSource<string>? previousRead;
        CancellationTokenRegistration previousReadCancellation;
        TaskCompletionSource<string> pendingRead = new(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            previousRead = _pendingRead;
            previousReadCancellation = _pendingReadCancellation;
            _pendingRead = pendingRead;
            _pendingReadCancellation = cancellationToken.Register(static state =>
            {
                var registration = (PendingReadRegistration)state!;
                registration.Reader.CancelPendingRead(registration.PendingRead);
            }, new PendingReadRegistration(this, pendingRead));
        }

        previousReadCancellation.Dispose();
        previousRead?.TrySetCanceled();

        return pendingRead.Task;
    }

    public IAsyncEnumerable<string> QrReadingNotifications(CancellationToken cancellationToken = default) => _readings.Reader.ReadAllAsync(cancellationToken);

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data;
        try
        {
            data = _serialPort.ReadExisting();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (data.Length == 0)
            return;

        foreach (string reading in ExtractReadings(data))
            PublishReading(reading);
    }

    private IReadOnlyList<string> ExtractReadings(string data)
    {
        List<string> readings = [];

        lock (_gate)
        {
            _buffer.Append(data);

            while (TryReadLine(_buffer, out string reading))
            {
                string normalizedReading = NormalizeReading(reading);
                if (!string.IsNullOrWhiteSpace(normalizedReading))
                    readings.Add(normalizedReading);
            }
        }

        return readings;
    }

    private void PublishReading(string reading)
    {
        _readings.Writer.TryWrite(reading);

        TaskCompletionSource<string>? pendingRead;
        CancellationTokenRegistration pendingReadCancellation;

        lock (_gate)
        {
            pendingRead = _pendingRead;
            pendingReadCancellation = _pendingReadCancellation;
            _pendingRead = null;
            _pendingReadCancellation = default;
        }

        pendingReadCancellation.Dispose();
        pendingRead?.TrySetResult(reading);
    }

    private void CancelPendingRead(TaskCompletionSource<string> expectedPendingRead)
    {
        TaskCompletionSource<string>? pendingRead;

        lock (_gate)
        {
            if (!ReferenceEquals(_pendingRead, expectedPendingRead))
                return;

            pendingRead = _pendingRead;
            _pendingRead = null;
            _pendingReadCancellation = default;
        }

        pendingRead?.TrySetCanceled();
    }

    private static bool TryReadLine(StringBuilder buffer, out string line)
    {
        for (int index = 0; index < buffer.Length; index++)
        {
            char character = buffer[index];
            if (character is not ('\r' or '\n'))
                continue;

            line = buffer.ToString(0, index);

            int removeLength = index + 1;
            if (character == '\r' && removeLength < buffer.Length && buffer[removeLength] == '\n')
                removeLength++;

            buffer.Remove(0, removeLength);
            return true;
        }

        line = string.Empty;
        return false;
    }

    private static string NormalizeReading(string reading) => reading.StartsWith(@"\000026", StringComparison.Ordinal) ? reading[7..] : reading;

    private sealed record PendingReadRegistration(QrReader Reader, TaskCompletionSource<string> PendingRead);
}
