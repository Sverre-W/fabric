using System.Threading.Channels;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public sealed record VisitorPreOnboardingSagaWorkItem(string TenantId, Guid SagaId);
public sealed record VisitorPreOnboardingSagaEventWorkItem(string TenantId, Guid EventId);

public sealed class VisitorPreOnboardingSagaTrigger
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.DropWrite,
    });

    public void Notify() =>
        _channel.Writer.TryWrite(true);

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken) =>
        _channel.Reader.WaitToReadAsync(cancellationToken);

    public bool TryRead() =>
        _channel.Reader.TryRead(out _);
}
