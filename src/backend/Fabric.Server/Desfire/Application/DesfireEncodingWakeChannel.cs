using System.Threading.Channels;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireEncodingWakeChannel
{
    private readonly Channel<bool> _channel = Channel.CreateUnbounded<bool>();

    public void Signal() => _channel.Writer.TryWrite(true);

    public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (_channel.Reader.TryRead(out _))
        {
            DrainSignals();
            return true;
        }

        using CancellationTokenSource timeoutSource = new(timeout);
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        try
        {
            bool hasData = await _channel.Reader.WaitToReadAsync(linked.Token);
            if (!hasData)
                return false;

            DrainSignals();
            return true;
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private void DrainSignals()
    {
        while (_channel.Reader.TryRead(out _))
        {
        }
    }
}
