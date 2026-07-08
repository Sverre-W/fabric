using System.Text.Json.Nodes;
using Fabric.Hardware.Agent.Devices;
using Fabric.Hardware.Agent.Gateway;
using Fabric.Hardware.Agent.Options;
using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Contracts.Rfid;
using Microsoft.Extensions.Options;

namespace Fabric.Hardware.Agent;

public sealed class CommandWorker(
    HardwareGatewayClient gatewayClient,
    IReadOnlyList<IQrReaderDevice> qrReaders,
    IReadOnlyList<IDispenserDevice> dispensers,
    IReadOnlyList<ICollectorDevice> collectors,
    IReadOnlyList<IEncoderDevice> encoders,
    IReadOnlyList<IRfidReaderDevice> rfidReaders,
    TimeProvider timeProvider,
    IOptions<HardwareAgentOptions> options,
    ILogger<CommandWorker> logger) : BackgroundService
{
    private readonly SemaphoreSlim _drainLock = new(1, 1);
    private readonly IReadOnlyDictionary<string, ICollectorDevice> _collectors = collectors.ToDictionary(collector => collector.DeviceId, StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyDictionary<string, IDispenserDevice> _dispensers = dispensers.ToDictionary(dispenser => dispenser.DeviceId, StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyDictionary<string, IEncoderDevice> _encoders = encoders.ToDictionary(encoder => encoder.DeviceId, StringComparer.OrdinalIgnoreCase);
    private readonly HardwareAgentOptions _options = options.Value;
    private readonly IReadOnlyDictionary<string, IQrReaderDevice> _qrReaders = qrReaders.ToDictionary(qrReader => qrReader.DeviceId, StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyDictionary<string, IRfidReaderDevice> _rfidReaders = rfidReaders.ToDictionary(rfidReader => rfidReader.DeviceId, StringComparer.OrdinalIgnoreCase);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.WorkerStarted(nameof(CommandWorker));

        Task streamTask = RunCommandStreamAsync(stoppingToken);
        Task reconcileTask = RunReconciliationAsync(stoppingToken);

        try
        {
            await Task.WhenAll(streamTask, reconcileTask);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }

        logger.WorkerStopped(nameof(CommandWorker));
    }

    private async Task RunCommandStreamAsync(CancellationToken stoppingToken)
    {
        int failures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.CommandStreamConnected();
                failures = 0;

                await foreach (Guid _ in gatewayClient.StreamCommandNotificationsAsync(stoppingToken))
                    await DrainCommandsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                failures++;
                logger.CommandStreamDisconnected(ex);
                await PollUntilReconnectAsync(GetReconnectDelay(failures), stoppingToken);
            }
        }
    }

    private async Task RunReconciliationAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_options.CommandReconcileInterval, stoppingToken);
            await DrainCommandsAsync(stoppingToken);
        }
    }

    private async Task PollUntilReconnectAsync(TimeSpan reconnectDelay, CancellationToken stoppingToken)
    {
        using var reconnectTimeout = new CancellationTokenSource(reconnectDelay);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, reconnectTimeout.Token);

        while (!linkedCancellation.IsCancellationRequested)
        {
            try
            {
                await DrainCommandsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.CommandPollFailed(ex);
            }

            try
            {
                await Task.Delay(_options.CommandPollInterval, linkedCancellation.Token);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task DrainCommandsAsync(CancellationToken stoppingToken)
    {
        await _drainLock.WaitAsync(stoppingToken);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                HardwareCommandEnvelope? command = await gatewayClient.GetNextCommandAsync(stoppingToken);
                if (command is null)
                    return;

                await ProcessCommandAsync(command, stoppingToken);
            }
        }
        finally
        {
            _drainLock.Release();
        }
    }

    private async Task ProcessCommandAsync(HardwareCommandEnvelope command, CancellationToken stoppingToken)
    {
        HardwareCommandClaimResponse? claim = await gatewayClient.ClaimCommandAsync(command.CommandId, stoppingToken);
        if (claim is null)
        {
            logger.CommandClaimLost(command.CommandId);
            return;
        }

        PostHardwareCommandResultRequest result = await ExecuteCommandAsync(claim.Command, stoppingToken);
        await gatewayClient.PostCommandResultAsync(command.CommandId, result, stoppingToken);
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteCommandAsync(HardwareCommandEnvelope command, CancellationToken stoppingToken)
    {
        if (_qrReaders.TryGetValue(command.DeviceId, out IQrReaderDevice? qrReader))
            return await ExecuteQrCommandAsync(command, qrReader, stoppingToken);

        if (_dispensers.TryGetValue(command.DeviceId, out IDispenserDevice? dispenser))
            return await ExecuteDispenserCommandAsync(command, dispenser, stoppingToken);

        if (_collectors.TryGetValue(command.DeviceId, out ICollectorDevice? collector))
            return await ExecuteCollectorCommandAsync(command, collector, stoppingToken);

        if (_encoders.TryGetValue(command.DeviceId, out IEncoderDevice? encoder))
            return await ExecuteEncoderCommandAsync(command, encoder, stoppingToken);

        if (_rfidReaders.TryGetValue(command.DeviceId, out IRfidReaderDevice? rfidReader))
            return await ExecuteRfidCommandAsync(command, rfidReader, stoppingToken);

        return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "Device is not available on this agent.");
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteRfidCommandAsync(HardwareCommandEnvelope command, IRfidReaderDevice rfidReader, CancellationToken stoppingToken)
    {
        if (!string.Equals(command.Capability, HardwareCapabilities.RfidRead, StringComparison.OrdinalIgnoreCase))
            return Failure(HardwareOperationStatus.Failed, "unsupported_capability", "Capability is not supported by this device.");

        using var timeout = new CancellationTokenSource(_options.CommandTimeout);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

        try
        {
            string cardNumber = await rfidReader.ReadCardAsync(linkedCancellation.Token);
            var result = new JsonObject { ["cardNumber"] = cardNumber };
            return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Timeout, "timeout", "RFID read timed out.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Cancelled, "cancelled", "RFID read was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, rfidReader.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "RFID reader is not available.");
        }
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteEncoderCommandAsync(HardwareCommandEnvelope command, IEncoderDevice encoder, CancellationToken stoppingToken)
    {
        using var timeout = new CancellationTokenSource(_options.CommandTimeout);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

        try
        {
            if (string.Equals(command.Capability, HardwareCapabilities.CardPresent, StringComparison.OrdinalIgnoreCase))
            {
                await encoder.WaitForCardPresentAsync(linkedCancellation.Token);
                var result = new JsonObject { ["present"] = true };
                return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
            }

            if (string.Equals(command.Capability, HardwareCapabilities.RfidApduExchange, StringComparison.OrdinalIgnoreCase))
                return await ExchangeEncoderApduAsync(command.Payload, encoder, linkedCancellation.Token);

            if (string.Equals(command.Capability, HardwareCapabilities.CardEject, StringComparison.OrdinalIgnoreCase))
            {
                await encoder.WaitForCardRemovalAsync(linkedCancellation.Token);
                var result = new JsonObject { ["removed"] = true };
                return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
            }

            return Failure(HardwareOperationStatus.Failed, "unsupported_capability", "Capability is not supported by this device.");
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Timeout, "timeout", "Encoder command timed out.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Cancelled, "cancelled", "Encoder command was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, encoder.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", ex.Message);
        }
    }

    private async Task<PostHardwareCommandResultRequest> ExchangeEncoderApduAsync(JsonObject? payload, IEncoderDevice encoder, CancellationToken cancellationToken)
    {
        if (!TryReadApduRequest(payload, out RfidApduExchangeRequest? request, out PostHardwareCommandResultRequest? invalidPayload))
            return invalidPayload!;

        byte[] response = await encoder.ExchangeApduAsync(Convert.FromHexString(request!.CommandHex), cancellationToken);
        var result = new JsonObject { ["responseHex"] = Convert.ToHexString(response) };
        return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteQrCommandAsync(HardwareCommandEnvelope command, IQrReaderDevice qrReader, CancellationToken stoppingToken)
    {
        if (!string.Equals(command.Capability, HardwareCapabilities.QrScan, StringComparison.OrdinalIgnoreCase))
            return Failure(HardwareOperationStatus.Failed, "unsupported_capability", "Capability is not supported by this device.");

        using var timeout = new CancellationTokenSource(_options.CommandTimeout);
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

        try
        {
            string value = await qrReader.ReadQrCodeAsync(linkedCancellation.Token);
            var result = new JsonObject { ["value"] = value };
            return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Timeout, "timeout", "QR scan timed out.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Cancelled, "cancelled", "QR scan was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, qrReader.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "QR reader is not available.");
        }
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteDispenserCommandAsync(HardwareCommandEnvelope command, IDispenserDevice dispenser, CancellationToken stoppingToken)
    {
        if (string.Equals(command.Capability, HardwareCapabilities.CardDrop, StringComparison.OrdinalIgnoreCase))
            return await DropCardAsync(command, dispenser, stoppingToken);

        if (string.Equals(command.Capability, HardwareCapabilities.CardPresent, StringComparison.OrdinalIgnoreCase))
            return await PresentCardAsync(command, dispenser, dropAfterRead: false, stoppingToken);

        if (string.Equals(command.Capability, HardwareCapabilities.CardDispense, StringComparison.OrdinalIgnoreCase))
            return await PresentCardAsync(command, dispenser, dropAfterRead: true, stoppingToken);

        return Failure(HardwareOperationStatus.Failed, "unsupported_capability", "Capability is not supported by this device.");
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteCollectorCommandAsync(HardwareCommandEnvelope command, ICollectorDevice collector, CancellationToken stoppingToken)
    {
        if (string.Equals(command.Capability, HardwareCapabilities.CardPresent, StringComparison.OrdinalIgnoreCase))
            return await PresentCollectorCardAsync(command, collector, stoppingToken);

        if (string.Equals(command.Capability, HardwareCapabilities.CardCollect, StringComparison.OrdinalIgnoreCase))
            return await CollectCardAsync(command, collector, stoppingToken);

        if (string.Equals(command.Capability, HardwareCapabilities.CardEject, StringComparison.OrdinalIgnoreCase))
            return await EjectCardAsync(command, collector, stoppingToken);

        return Failure(HardwareOperationStatus.Failed, "unsupported_capability", "Capability is not supported by this device.");
    }

    private async Task<PostHardwareCommandResultRequest> DropCardAsync(HardwareCommandEnvelope command, IDispenserDevice dispenser, CancellationToken stoppingToken)
    {
        try
        {
            bool dropped = await dispenser.DropAsync(stoppingToken);
            if (!dropped)
            {
                logger.DispenserCommandFailed(dispenser.DeviceId, command.Capability);
                return Failure(HardwareOperationStatus.Failed, "drop_failed", "Dispenser did not confirm card drop.");
            }

            var result = new JsonObject { ["dropped"] = true };
            return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, dispenser.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "Dispenser is not available.");
        }
    }

    private async Task<PostHardwareCommandResultRequest> PresentCollectorCardAsync(HardwareCommandEnvelope command, ICollectorDevice collector, CancellationToken stoppingToken)
    {
        if (!_rfidReaders.TryGetValue(collector.RfidReaderDeviceId, out IRfidReaderDevice? rfidReader))
            return Failure(HardwareOperationStatus.DeviceUnavailable, "rfid_reader_unavailable", "Configured RFID reader is not available on this agent.");

        try
        {
            await collector.WaitForCardAtReaderAsync(stoppingToken);

            using var timeout = new CancellationTokenSource(collector.RfidReadTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

            try
            {
                string cardNumber = await rfidReader.ReadCardAsync(linkedCancellation.Token);
                var result = new JsonObject { ["cardNumber"] = cardNumber };
                return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
            {
                return Failure(HardwareOperationStatus.Timeout, "rfid_read_timeout", "RFID read timed out.");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Cancelled, "cancelled", "Card present operation was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, collector.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "Collector or RFID reader is not available.");
        }
    }

    private async Task<PostHardwareCommandResultRequest> CollectCardAsync(HardwareCommandEnvelope command, ICollectorDevice collector, CancellationToken stoppingToken)
    {
        if (!TryReadPlaceInCollectorStack(command.Payload, out bool placeInCollectorStack, out PostHardwareCommandResultRequest? invalidPayload))
            return invalidPayload!;

        try
        {
            bool collected = await collector.CollectAsync(placeInCollectorStack, stoppingToken);
            if (!collected)
            {
                logger.CollectorCommandFailed(collector.DeviceId, command.Capability);
                return Failure(HardwareOperationStatus.Failed, "collect_failed", "Collector did not confirm card collect.");
            }

            var result = new JsonObject
            {
                ["collected"] = true,
                ["placeInCollectorStack"] = placeInCollectorStack
            };
            return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, collector.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "Collector is not available.");
        }
    }

    private async Task<PostHardwareCommandResultRequest> EjectCardAsync(HardwareCommandEnvelope command, ICollectorDevice collector, CancellationToken stoppingToken)
    {
        try
        {
            bool ejected = await collector.EjectAsync(stoppingToken);
            if (!ejected)
            {
                logger.CollectorCommandFailed(collector.DeviceId, command.Capability);
                return Failure(HardwareOperationStatus.Failed, "eject_failed", "Collector did not confirm card eject.");
            }

            using var removalTimeout = new CancellationTokenSource(collector.RemovalTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, removalTimeout.Token);

            try
            {
                await collector.WaitForCardRemovalAsync(linkedCancellation.Token);
                var removed = new JsonObject
                {
                    ["removed"] = true,
                    ["recollected"] = false
                };
                return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, removed, null, timeProvider.GetUtcNow());
            }
            catch (OperationCanceledException) when (removalTimeout.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
            {
                bool recollected = await collector.CollectAsync(placeInCollectorStack: true, stoppingToken);
                var timeoutResult = new JsonObject
                {
                    ["removed"] = false,
                    ["recollected"] = recollected
                };
                return new PostHardwareCommandResultRequest(
                    HardwareOperationStatus.Timeout,
                    timeoutResult,
                    new HardwareErrorResponse("card_removal_timeout", "Card was not removed before timeout."),
                    timeProvider.GetUtcNow());
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Cancelled, "cancelled", "Card eject operation was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, collector.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "Collector is not available.");
        }
    }

    private async Task<PostHardwareCommandResultRequest> PresentCardAsync(HardwareCommandEnvelope command, IDispenserDevice dispenser, bool dropAfterRead, CancellationToken stoppingToken)
    {
        if (!_rfidReaders.TryGetValue(dispenser.RfidReaderDeviceId, out IRfidReaderDevice? rfidReader))
            return Failure(HardwareOperationStatus.DeviceUnavailable, "rfid_reader_unavailable", "Configured RFID reader is not available on this agent.");

        try
        {
            bool dispensed = await dispenser.DispenseAsync(stoppingToken);
            if (!dispensed)
            {
                logger.DispenserCommandFailed(dispenser.DeviceId, dropAfterRead ? HardwareCapabilities.CardDispense : HardwareCapabilities.CardPresent);
                return Failure(HardwareOperationStatus.Failed, "dispense_failed", "Dispenser did not confirm card dispense.");
            }

            using var timeout = new CancellationTokenSource(dispenser.RfidReadTimeout);
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);

            try
            {
                string cardNumber = await rfidReader.ReadCardAsync(linkedCancellation.Token);
                if (dropAfterRead && !await DropAfterReadAsync(dispenser, stoppingToken))
                    return Failure(HardwareOperationStatus.Failed, "drop_failed", "Card was read, but dispenser did not confirm card drop.");

                var result = new JsonObject { ["cardNumber"] = cardNumber };
                return new PostHardwareCommandResultRequest(HardwareOperationStatus.Succeeded, result, null, timeProvider.GetUtcNow());
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
            {
                await DropAfterReadAsync(dispenser, stoppingToken);
                return Failure(HardwareOperationStatus.Timeout, "rfid_read_timeout", "RFID read timed out.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await DropAfterReadAsync(dispenser, stoppingToken);
                logger.CardPresentReadFailed(dispenser.DeviceId, rfidReader.DeviceId, ex);
                return Failure(HardwareOperationStatus.Failed, "rfid_read_failed", "RFID read failed.");
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return Failure(HardwareOperationStatus.Cancelled, "cancelled", "Card operation was cancelled.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.CommandDeviceUnavailable(command.CommandId, dispenser.DeviceId, ex);
            return Failure(HardwareOperationStatus.DeviceUnavailable, "device_unavailable", "Dispenser or RFID reader is not available.");
        }
    }

    private async Task<bool> DropAfterReadAsync(IDispenserDevice dispenser, CancellationToken cancellationToken)
    {
        try
        {
            bool dropped = await dispenser.DropAsync(cancellationToken);
            if (!dropped)
                logger.DispenserCommandFailed(dispenser.DeviceId, HardwareCapabilities.CardDrop);

            return dropped;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            logger.DispenserDropAfterReadFailureFailed(dispenser.DeviceId, ex);
            return false;
        }
    }

    private PostHardwareCommandResultRequest Failure(HardwareOperationStatus status, string code, string message) =>
        new(status, null, new HardwareErrorResponse(code, message), timeProvider.GetUtcNow());

    private bool TryReadApduRequest(JsonObject? payload, out RfidApduExchangeRequest? request, out PostHardwareCommandResultRequest? error)
    {
        request = null;
        error = null;

        if (payload is null)
        {
            error = Failure(HardwareOperationStatus.Failed, "invalid_payload", "commandHex is required.");
            return false;
        }

        try
        {
            string commandHex = payload["commandHex"]?.GetValue<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(commandHex))
            {
                error = Failure(HardwareOperationStatus.Failed, "invalid_payload", "commandHex is required.");
                return false;
            }

            Convert.FromHexString(commandHex);
            request = new RfidApduExchangeRequest(commandHex);
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            error = Failure(HardwareOperationStatus.Failed, "invalid_payload", "commandHex must be a valid hex string.");
            return false;
        }
    }

    private bool TryReadPlaceInCollectorStack(JsonObject? payload, out bool placeInCollectorStack, out PostHardwareCommandResultRequest? error)
    {
        placeInCollectorStack = true;
        error = null;

        if (payload?["placeInCollectorStack"] is null)
            return true;

        try
        {
            placeInCollectorStack = payload["placeInCollectorStack"]!.GetValue<bool>();
            return true;
        }
        catch (InvalidOperationException)
        {
            error = Failure(HardwareOperationStatus.Failed, "invalid_payload", "placeInCollectorStack must be a boolean value.");
            return false;
        }
    }

    private static TimeSpan GetReconnectDelay(int failures) => failures switch
    {
        <= 1 => TimeSpan.FromSeconds(1),
        2 => TimeSpan.FromSeconds(2),
        3 => TimeSpan.FromSeconds(5),
        4 => TimeSpan.FromSeconds(10),
        _ => TimeSpan.FromSeconds(30)
    };
}
