using System.Threading.Channels;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public sealed record VisitorPreOnboardingSagaWorkItem(string TenantId, Guid SagaId);

public sealed class VisitorPreOnboardingSagaTrigger
{
    private readonly Channel<VisitorPreOnboardingSagaWorkItem> _channel = Channel.CreateUnbounded<VisitorPreOnboardingSagaWorkItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    public ValueTask EnqueueAsync(VisitorPreOnboardingSagaWorkItem workItem, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(workItem, cancellationToken);

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken) =>
        _channel.Reader.WaitToReadAsync(cancellationToken);

    public bool TryRead(out VisitorPreOnboardingSagaWorkItem? workItem) =>
        _channel.Reader.TryRead(out workItem);
}
