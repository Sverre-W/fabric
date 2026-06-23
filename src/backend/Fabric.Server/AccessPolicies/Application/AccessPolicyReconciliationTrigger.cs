using System.Threading.Channels;

namespace Fabric.Server.AccessPolicies.Application;

public sealed record AccessPolicyReconciliationWorkItem(string TenantId, Guid SubjectId, Guid SystemId);

public sealed class AccessPolicyReconciliationTrigger
{
    private readonly Channel<AccessPolicyReconciliationWorkItem> _channel = Channel.CreateUnbounded<AccessPolicyReconciliationWorkItem>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false,
    });

    public ValueTask EnqueueAsync(AccessPolicyReconciliationWorkItem workItem, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(workItem, cancellationToken);

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken) =>
        _channel.Reader.WaitToReadAsync(cancellationToken);

    public bool TryRead(out AccessPolicyReconciliationWorkItem? workItem) =>
        _channel.Reader.TryRead(out workItem);
}
