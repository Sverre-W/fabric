using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Collector;

public sealed class CollectorSettings
{
    public int MaxJamCount { get; init; } = 3;
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromMilliseconds(250);
    public TimeSpan AckTimeout { get; init; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan MechanicalAckTimeout { get; init; } = TimeSpan.FromSeconds(3);
    public required string ComPort { get; init; }
}

public sealed record StateTransition(CollectorState PreviousState, CollectorState NewState);

public enum CollectorState
{
    Idle,
    CardAtReader,
    CardCollected,
    CardCaptured,
    Ejected,
    Jammed
}

public sealed class Collector : IDisposable
{
    private readonly CollectorComPort _collector;
    private readonly ConcurrentQueue<CollectorSensorState> _collectorSensorStates = new();
    private readonly Channel<StateTransition> _stateTransitions = Channel.CreateUnbounded<StateTransition>(new UnboundedChannelOptions
    {
        AllowSynchronousContinuations = false,
        SingleReader = false,
        SingleWriter = true
    });
    private readonly CancellationTokenSource _stopping = new();
    private readonly ILogger<Collector> _logger;
    private readonly CollectorSettings _settings;
    private readonly Task _pollingTask;
    private readonly object _gate = new();

    private TaskCompletionSource? _cardAtReaderCompletionSource;
    private TaskCompletionSource? _cardRemovedCompletionSource;
    private bool _cardAtReader;
    private bool _disposed;
    private int _jamCount;
    private CollectorState _previousState = CollectorState.Idle;

    public Collector(CollectorSettings settings, ILogger<Collector> logger, ILogger<CollectorComPort> collectorLogger)
    {
        _settings = settings;
        _logger = logger;

        _collector = new CollectorComPort(collectorLogger, settings.ComPort, settings.AckTimeout, settings.MechanicalAckTimeout)
        {
            OnStateReceived = _collectorSensorStates.Enqueue,
        };

        _pollingTask = PollState(_stopping.Token);
    }

    public async Task<bool> CollectAsync(CancellationToken cancellationToken = default)
    {
        bool acknowledged = await DoCommand(CollectorCommand.CollectCard, CollectorState.CardCollected, cancellationToken);
        if (acknowledged)
            ClearCardAtReader();

        return acknowledged;
    }

    public async Task<bool> CaptureAsync(CancellationToken cancellationToken = default)
    {
        bool acknowledged = await DoCommand(CollectorCommand.CaptureCard, CollectorState.CardCaptured, cancellationToken);
        if (acknowledged)
            ClearCardAtReader();

        return acknowledged;
    }

    public async Task<bool> EjectAsync(CancellationToken cancellationToken = default)
    {
        bool acknowledged = await DoCommand(CollectorCommand.FeedHold, CollectorState.Ejected, cancellationToken);
        if (acknowledged)
            SetCardAtReader();

        return acknowledged;
    }

    public Task WaitForCardAtReaderAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_cardAtReader)
                return Task.CompletedTask;

            _cardAtReaderCompletionSource ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _cardAtReaderCompletionSource.Task.WaitAsync(cancellationToken);
        }
    }

    public Task WaitForCardRemovalAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_cardAtReader)
                return Task.CompletedTask;

            _cardRemovedCompletionSource ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _cardRemovedCompletionSource.Task.WaitAsync(cancellationToken);
        }
    }

    public IAsyncEnumerable<StateTransition> ReadStateTransitionsAsync(CancellationToken cancellationToken = default) =>
        _stateTransitions.Reader.ReadAllAsync(cancellationToken);

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stopping.Cancel();
        _stateTransitions.Writer.TryComplete();

        lock (_gate)
        {
            _cardAtReaderCompletionSource?.TrySetCanceled();
            _cardRemovedCompletionSource?.TrySetCanceled();
            _cardAtReaderCompletionSource = null;
            _cardRemovedCompletionSource = null;
        }

        _collector.Dispose();
        _stopping.Dispose();
    }

    private async Task<bool> DoCommand(CollectorCommand command, CollectorState transition, CancellationToken cancellationToken)
    {
        _logger.CollectorCommandExecuting(command);
        CollectorAcknowledge acknowledge = await _collector.ExecuteCommand(command, cancellationToken);
        if (acknowledge != CollectorAcknowledge.Ack)
        {
            _logger.CollectorCommandFailed(command, acknowledge);
            return false;
        }

        Transition(transition);
        return true;
    }

    private void ProcessState(CollectorSensorState previousState, CollectorSensorState newState)
    {
        if (newState.Busy)
        {
            _logger.CollectorBusy();
            return;
        }

        if (newState.Jammed)
        {
            _jamCount++;

            if (_jamCount > _settings.MaxJamCount)
            {
                _logger.CollectorMaxJamRetriesReached();
                Transition(CollectorState.Jammed);
            }
            else
            {
                _ = ClearJam(_stopping.Token);
            }
        }

        if (previousState.CardBeingCollected && newState.CardBeingCollected)
        {
            _jamCount++;
            if (_jamCount < _settings.MaxJamCount)
                _ = _collector.ExecuteCommand(CollectorCommand.CollectCard, _stopping.Token);
            else
                Transition(CollectorState.Jammed);

            return;
        }

        _jamCount = 0;

        if (previousState.Equals(newState))
            return;

        if (newState.CollectStackFull)
        {
            _logger.CollectorStackFull();
            Transition(CollectorState.Jammed);
        }

        if (newState.HasTwoCards)
        {
            _logger.CollectorTwoCardsDetected();
            Transition(CollectorState.Jammed);
        }

        if (newState.AnyFeedSensor && previousState.HasTwoCards)
            _ = _collector.ExecuteCommand(CollectorCommand.FeedHold, _stopping.Token);

        if (previousState.Clear && newState.IsFeedSensor1(true))
        {
            _logger.CollectorCardDetected();
            _ = _collector.ExecuteCommand(CollectorCommand.FeedStandBy, _stopping.Token);
        }

        if (newState.IsFeedSensor2(true))
        {
            _logger.CollectorCardAtReader();
            SetCardAtReader();
            Transition(CollectorState.CardAtReader);
        }

        if (newState.Clear)
        {
            Transition(CollectorState.Idle);
            ClearCardAtReader();
        }
    }

    private async Task ClearJam(CancellationToken cancellationToken)
    {
        _logger.CollectorClearingJam();

        await _collector.ExecuteCommand(CollectorCommand.ClearJam, cancellationToken);
        await Task.Delay(_settings.PollingInterval, cancellationToken);
        await _collector.ExecuteCommand(CollectorCommand.FeedStandBy, cancellationToken);
    }

    private async Task PollState(CancellationToken cancellationToken)
    {
        CollectorSensorState previousState = CollectorSensorState.ClearState;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_collectorSensorStates.TryDequeue(out CollectorSensorState? newState))
                {
                    ProcessState(previousState, newState);
                    previousState = newState;
                    continue;
                }

                await _collector.ExecuteCommand(CollectorCommand.ReadStatus, cancellationToken);
                await Task.Delay(_settings.PollingInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void Transition(CollectorState newState)
    {
        if (newState == _previousState)
            return;

        var stateChange = new StateTransition(_previousState, newState);
        _previousState = newState;

        _logger.CollectorStateChanged(stateChange);
        _stateTransitions.Writer.TryWrite(stateChange);
    }

    private void SetCardAtReader()
    {
        TaskCompletionSource? pending;
        lock (_gate)
        {
            _cardAtReader = true;
            pending = _cardAtReaderCompletionSource;
            _cardAtReaderCompletionSource = null;
        }

        pending?.TrySetResult();
    }

    private void ClearCardAtReader()
    {
        TaskCompletionSource? pending;
        lock (_gate)
        {
            if (!_cardAtReader)
                return;

            _cardAtReader = false;
            pending = _cardRemovedCompletionSource;
            _cardRemovedCompletionSource = null;
        }

        pending?.TrySetResult();
    }
}
