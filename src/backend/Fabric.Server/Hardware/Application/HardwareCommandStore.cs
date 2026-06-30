using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Commands;

namespace Fabric.Server.Hardware.Application;

public sealed class HardwareCommandStore(TimeProvider timeProvider)
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<Guid, PendingHardwareCommand> _commands = new();

    public PendingHardwareCommand Create(string agentId, string deviceId, string capability, JsonObject? payload, TimeSpan timeout)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        var command = new PendingHardwareCommand(
            Guid.NewGuid(),
            agentId,
            deviceId,
            capability,
            now,
            now.Add(timeout),
            payload);

        _commands[command.CommandId] = command;
        return command;
    }

    public PendingHardwareCommand? GetNext(string agentId)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        foreach (PendingHardwareCommand command in _commands.Values)
        {
            if (!command.AgentId.Equals(agentId, StringComparison.OrdinalIgnoreCase))
                continue;

            if (command.TryExpire(now))
                continue;

            if (command.Status == HardwareCommandStatus.Pending)
                return command;
        }

        return null;
    }

    public bool TryClaim(Guid commandId, string agentId, out HardwareCommandClaimResponse? response)
    {
        response = null;
        if (!_commands.TryGetValue(commandId, out PendingHardwareCommand? command))
            return false;

        DateTimeOffset now = timeProvider.GetUtcNow();
        if (!command.TryClaim(agentId, now, now.Add(LeaseDuration)))
            return false;

        response = new HardwareCommandClaimResponse(command.ToEnvelope(), command.LeaseExpiresAt!.Value);
        return true;
    }

    public bool TryComplete(Guid commandId, string agentId, PostHardwareCommandResultRequest request)
    {
        if (!_commands.TryGetValue(commandId, out PendingHardwareCommand? command))
            return false;

        return command.TryComplete(agentId, request);
    }

    public async Task<PostHardwareCommandResultRequest> WaitForResultAsync(
        PendingHardwareCommand command,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeout = new(command.ExpiresAt - timeProvider.GetUtcNow());
        using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        try
        {
            return await command.Result.Task.WaitAsync(linked.Token);
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            var result = new PostHardwareCommandResultRequest(
                HardwareOperationStatus.Timeout,
                null,
                new HardwareErrorResponse("timeout", "Hardware command timed out."),
                timeProvider.GetUtcNow());

            command.TryTimeout(result);
            return result;
        }
    }
}

public sealed class PendingHardwareCommand(
    Guid commandId,
    string agentId,
    string deviceId,
    string capability,
    DateTimeOffset createdAt,
    DateTimeOffset expiresAt,
    JsonObject? payload)
{
    private readonly object _sync = new();

    public Guid CommandId { get; } = commandId;
    public string AgentId { get; } = agentId;
    public string DeviceId { get; } = deviceId;
    public string Capability { get; } = capability;
    public DateTimeOffset CreatedAt { get; } = createdAt;
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
    public JsonObject? Payload { get; } = payload;
    public HardwareCommandStatus Status { get; private set; } = HardwareCommandStatus.Pending;
    public DateTimeOffset? LeaseExpiresAt { get; private set; }
    public TaskCompletionSource<PostHardwareCommandResultRequest> Result { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public HardwareCommandEnvelope ToEnvelope() => new(CommandId, DeviceId, Capability, CreatedAt, ExpiresAt, Payload);

    public bool TryClaim(string agentId, DateTimeOffset now, DateTimeOffset leaseExpiresAt)
    {
        lock (_sync)
        {
            if (!AgentId.Equals(agentId, StringComparison.OrdinalIgnoreCase) || Status != HardwareCommandStatus.Pending || now >= ExpiresAt)
                return false;

            Status = HardwareCommandStatus.Claimed;
            LeaseExpiresAt = leaseExpiresAt;
            return true;
        }
    }

    public bool TryComplete(string agentId, PostHardwareCommandResultRequest request)
    {
        lock (_sync)
        {
            if (!AgentId.Equals(agentId, StringComparison.OrdinalIgnoreCase)
                || Status is HardwareCommandStatus.Succeeded or HardwareCommandStatus.Timeout or HardwareCommandStatus.Cancelled or HardwareCommandStatus.DeviceUnavailable or HardwareCommandStatus.Busy or HardwareCommandStatus.Failed or HardwareCommandStatus.Expired)
                return false;

            Status = request.Status switch
            {
                HardwareOperationStatus.Succeeded => HardwareCommandStatus.Succeeded,
                HardwareOperationStatus.Timeout => HardwareCommandStatus.Timeout,
                HardwareOperationStatus.Cancelled => HardwareCommandStatus.Cancelled,
                HardwareOperationStatus.DeviceUnavailable => HardwareCommandStatus.DeviceUnavailable,
                HardwareOperationStatus.Busy => HardwareCommandStatus.Busy,
                HardwareOperationStatus.Failed => HardwareCommandStatus.Failed,
                _ => HardwareCommandStatus.Failed
            };

            Result.TrySetResult(request);
            return true;
        }
    }

    public bool TryExpire(DateTimeOffset now)
    {
        lock (_sync)
        {
            if (Status != HardwareCommandStatus.Pending || now < ExpiresAt)
                return false;

            Status = HardwareCommandStatus.Expired;
            Result.TrySetResult(new PostHardwareCommandResultRequest(
                HardwareOperationStatus.Timeout,
                null,
                new HardwareErrorResponse("expired", "Hardware command expired before it was claimed."),
                now));
            return true;
        }
    }

    public void TryTimeout(PostHardwareCommandResultRequest result)
    {
        lock (_sync)
        {
            if (Result.Task.IsCompleted)
                return;

            Status = HardwareCommandStatus.Timeout;
            Result.TrySetResult(result);
        }
    }
}
