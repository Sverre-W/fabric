using System.Collections.Concurrent;
using System.Threading.Channels;
using Fabric.Hardware.Contracts.Commands;

namespace Fabric.Server.Hardware.Application;

public sealed class HardwareAgentConnectionManager
{
    private readonly ConcurrentDictionary<string, Channel<HardwareCommandStreamEvent>> _channels = new(StringComparer.OrdinalIgnoreCase);

    public ChannelReader<HardwareCommandStreamEvent> Connect(string agentId)
    {
        Channel<HardwareCommandStreamEvent> channel = Channel.CreateUnbounded<HardwareCommandStreamEvent>();
        _channels[agentId] = channel;
        return channel.Reader;
    }

    public void Disconnect(string agentId)
    {
        if (_channels.TryRemove(agentId, out Channel<HardwareCommandStreamEvent>? channel))
            channel.Writer.TryComplete();
    }

    public void NotifyCommandAvailable(string agentId, Guid commandId)
    {
        Notify(agentId, new HardwareCommandStreamEvent(HardwareCommandEventType.CommandAvailable, commandId, null, null, null));
    }

    public void NotifyCommandCancelled(string agentId, Guid commandId, string deviceId, string capability, string? reason)
    {
        Notify(agentId, new HardwareCommandStreamEvent(HardwareCommandEventType.CommandCancelled, commandId, deviceId, capability, reason));
    }

    private void Notify(string agentId, HardwareCommandStreamEvent commandEvent)
    {
        if (_channels.TryGetValue(agentId, out Channel<HardwareCommandStreamEvent>? channel))
            channel.Writer.TryWrite(commandEvent);
    }
}
