using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.RfidEas.Infrastructure;

/// <summary>
///     A set of readers that have been connected and are available in the pcProxAPI.
/// </summary>
public sealed class RfidEasReader : IDisposable
{
    private readonly ICardDataReader _cardDataReader;
    private readonly ICardDataTransformer _dataTransformer;
    private readonly ConcurrentDictionary<int, TaskCompletionSource<string>> _listeners = new();
    private readonly CancellationTokenSource _stopping = new();

    private readonly ILogger<RfidEasReader> _logger;
    private readonly TimeSpan _pollingInterval;
    private readonly TimeSpan _readerDelay;
    private readonly Task _pollingTask;

    private bool _disposed;

    public RfidEasReader(
        ICardDataTransformer dataTransformer,
        ICardDataReader cardDataReader,
        ILogger<RfidEasReader> logger,
        TimeSpan? pollingInterval = null,
        TimeSpan? readerDelay = null
    )
    {
        _dataTransformer = dataTransformer;
        _cardDataReader = cardDataReader;
        _logger = logger;
        _pollingInterval = pollingInterval ?? TimeSpan.FromMilliseconds(250);
        _readerDelay = readerDelay ?? TimeSpan.FromMilliseconds(250);
        _pollingTask = Process(_stopping.Token);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stopping.Cancel();

        foreach (TaskCompletionSource<string> listener in _listeners.Values)
            listener.TrySetCanceled();

        _listeners.Clear();
        _stopping.Dispose();
    }

    public Task<string> ReadCard(int readerId, CancellationToken cancellationToken)
    {
        _logger.PreparingRead(readerId);
        TaskCompletionSource<string> listener = new(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_listeners.TryAdd(readerId, listener))
        {
            _logger.PendingReadAlreadyExists(readerId);
            throw new InvalidOperationException("RFID reader is busy.");
        }

        return WaitForCardAsync(readerId, listener, cancellationToken);
    }

    private async Task Process(CancellationToken cancellationToken)
    {
        IntPtr cardIdPtr = Marshal.AllocHGlobal(32 * sizeof(int));

        try
        {
            ReaderApi.SetDevTypeSrch(0);

            _logger.RfidReaderStarting();
            await ConnectReader(cancellationToken);

            short deviceCount = ReaderApi.GetDevCnt();
            int[] logicalIdMap = new int[deviceCount];
            _logger.MappingReaders(deviceCount);
            for (short device = 0; device < deviceCount; device++)
            {
                ReaderApi.SetActDev(device);
                logicalIdMap[device] = ReaderApi.GetLUID();
            }

            _logger.MappingDone();

            while (!cancellationToken.IsCancellationRequested)
            {
                for (short device = 0; device < deviceCount; device++)
                {
                    ReaderApi.SetActDev(device);

                    int bitCount = ReaderApi.GetActiveID32(cardIdPtr, 32);

                    if (bitCount == 0)
                    {
                        await Task.Delay(_pollingInterval, cancellationToken);
                        continue;
                    }

                    int byteCount = Math.Min(8, (bitCount + 7) / 8);
                    byte[] buffer = new byte[byteCount];

                    Marshal.Copy(cardIdPtr, buffer, 0, byteCount);
                    string data = _cardDataReader.ReadData(_dataTransformer.Transform(buffer));

                    int logicalId = logicalIdMap[device];

                    if (_listeners.Remove(logicalId, out TaskCompletionSource<string>? completionSource))
                    {
                        _logger.CardRead(logicalId, data);
                        completionSource.TrySetResult(data);
                    }

                    await Task.Delay(_readerDelay, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            Marshal.FreeHGlobal(cardIdPtr);
            _logger.RfidReaderStopped();
        }
    }

    private async Task ConnectReader(CancellationToken cancellationToken)
    {
        _logger.Connecting();
        while (!cancellationToken.IsCancellationRequested)
        {
            int rc = ReaderApi.usbConnect();
            if (rc == 1)
            {
                _logger.Connected();
                return;
            }

            TimeSpan timeout = _readerDelay * 8;
            _logger.ConnectRetry(timeout.TotalMilliseconds);
            await Task.Delay(timeout, cancellationToken);
        }
    }

    private async Task<string> WaitForCardAsync(int readerId, TaskCompletionSource<string> listener, CancellationToken cancellationToken)
    {
        try
        {
            return await listener.Task.WaitAsync(cancellationToken);
        }
        catch
        {
            if (_listeners.TryRemove(new KeyValuePair<int, TaskCompletionSource<string>>(readerId, listener)))
                listener.TrySetCanceled();

            throw;
        }
    }
}
