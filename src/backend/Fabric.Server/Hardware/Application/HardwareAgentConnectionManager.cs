using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Fabric.Server.Hardware.Application;

public sealed class HardwareAgentConnectionManager
{
    private readonly ConcurrentDictionary<string, Channel<Guid>> _channels = new(StringComparer.OrdinalIgnoreCase);

    public ChannelReader<Guid> Connect(string agentId)
    {
        Channel<Guid> channel = Channel.CreateUnbounded<Guid>();
        _channels[agentId] = channel;
        return channel.Reader;
    }

    public void Disconnect(string agentId)
    {
        if (_channels.TryRemove(agentId, out Channel<Guid>? channel))
            channel.Writer.TryComplete();
    }

    public void NotifyCommandAvailable(string agentId, Guid commandId)
    {
        if (_channels.TryGetValue(agentId, out Channel<Guid>? channel))
            channel.Writer.TryWrite(commandId);
    }
}
