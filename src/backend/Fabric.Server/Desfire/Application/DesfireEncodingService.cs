using System.Text.Json;
using System.Text.Json.Nodes;
using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Scripting;
using Fabric.Hardware.Desfire.Scripting.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Server.Desfire.Contracts;
using Fabric.Server.Desfire.Domain;
using Fabric.Server.Desfire.Persistence;
using Fabric.Server.Hardware.Application;
using Fabric.Server.Hardware.Domain;
using Fabric.Server.Hardware.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Desfire.Application;

public sealed class DesfireEncodingService(
    DesfireDbContext desfireDb,
    HardwareDbContext hardwareDb,
    DesfireTransformationPlanner planner,
    DesfireKeyGroupResolver keyGroupResolver,
    DesfireDeviceLeaseStore leaseStore,
    DesfireVariableResolver variableResolver,
    HardwareCommandStore commandStore,
    HardwareAgentConnectionManager connectionManager,
    TimeProvider timeProvider,
    ILogger<DesfireEncodingService> logger)
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ClaimDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CardWorkflowCommandTimeout = TimeSpan.FromMinutes(5);

    public async Task<DesfireEncodingResult> CreateAdHocAsync(CreateAdHocEncodingRequest request, CancellationToken ct)
    {
        Transformation? transformation = await desfireDb.Transformations.SingleOrDefaultAsync(candidate => candidate.Id == request.TransformationId, ct);
        if (transformation is null)
            return DesfireEncodingResult.NotFound;

        DateTimeOffset now = timeProvider.GetUtcNow();
        EncodingRun run = EncodingRun.Create(
            request.TransformationId,
            null,
            null,
            EncodingRunKind.AdHoc,
            JsonSerializer.Serialize(request.UserVariables, DesfireJson.Options),
            transformation.VariableConfigsJson,
            request.AgentId,
            request.DeviceId,
            request.Priority,
            now);
        desfireDb.EncodingRuns.Add(run);
        await desfireDb.SaveChangesAsync(ct);

        if (request.Mode == AdHocEncodingMode.Queued)
            return new DesfireEncodingResult(run, null);

        if (string.IsNullOrWhiteSpace(request.AgentId) || string.IsNullOrWhiteSpace(request.DeviceId))
        {
            run.Fail(EncodingRunStatus.Failed, "Sync ad-hoc encoding requires agentId and deviceId.", "[]", timeProvider.GetUtcNow());
            await desfireDb.SaveChangesAsync(ct);
            return new DesfireEncodingResult(run, Results.Problem(run.ErrorMessage, statusCode: StatusCodes.Status400BadRequest));
        }

        return await ExecuteRunAsync(run.Id, request.AgentId, request.DeviceId, requeueOnTransientFailure: false, ct);
    }

    public async Task<EncodingRun?> TryClaimNextRunForDeviceAsync(string agentId, string deviceId, string workerId, CancellationToken ct)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        string normalizedAgentId = agentId.Trim().ToLowerInvariant();
        string normalizedDeviceId = deviceId.Trim().ToLowerInvariant();

        Guid? runId = await desfireDb.EncodingRuns
            .AsNoTracking()
            .Where(candidate => (candidate.Status == EncodingRunStatus.Pending || (candidate.Status == EncodingRunStatus.Claimed && candidate.ClaimExpiresAt <= now))
                && (candidate.RequestedAgentId == null || candidate.RequestedAgentId == normalizedAgentId)
                && (candidate.RequestedDeviceId == null || candidate.RequestedDeviceId == normalizedDeviceId))
            .OrderByDescending(candidate => candidate.RequestedAgentId == normalizedAgentId && candidate.RequestedDeviceId == normalizedDeviceId)
            .ThenByDescending(candidate => candidate.Priority)
            .ThenBy(candidate => candidate.RequestedAt)
            .Select(candidate => (Guid?)candidate.Id)
            .FirstOrDefaultAsync(ct);

        if (runId is null)
            return null;

        DateTimeOffset expiresAt = now.Add(ClaimDuration);
        int updated = await desfireDb.EncodingRuns
            .Where(candidate => candidate.Id == runId
                && (candidate.Status == EncodingRunStatus.Pending || (candidate.Status == EncodingRunStatus.Claimed && candidate.ClaimExpiresAt <= now))
                && (candidate.RequestedAgentId == null || candidate.RequestedAgentId == normalizedAgentId)
                && (candidate.RequestedDeviceId == null || candidate.RequestedDeviceId == normalizedDeviceId))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(candidate => candidate.Status, EncodingRunStatus.Claimed)
                .SetProperty(candidate => candidate.ClaimedBy, workerId)
                .SetProperty(candidate => candidate.ClaimedAt, now)
                .SetProperty(candidate => candidate.ClaimExpiresAt, expiresAt)
                .SetProperty(candidate => candidate.AttemptCount, candidate => candidate.AttemptCount + 1), ct);

        if (updated == 0)
            return null;

        return await desfireDb.EncodingRuns.AsNoTracking().SingleAsync(candidate => candidate.Id == runId, ct);
    }

    public async Task<DesfireEncodingResult> ExecuteRunAsync(Guid runId, string agentId, string deviceId, bool requeueOnTransientFailure, CancellationToken ct)
    {
        EncodingRun? run = await desfireDb.EncodingRuns.SingleOrDefaultAsync(candidate => candidate.Id == runId, ct);
        if (run is null)
            return DesfireEncodingResult.NotFound;

        IResult? deviceFailure = await ValidateDeviceAsync(agentId, deviceId, ct);
        if (deviceFailure is not null)
        {
            if (requeueOnTransientFailure)
            {
                run.Requeue("Hardware device is unavailable or not ready.", timeProvider.GetUtcNow());
                await desfireDb.SaveChangesAsync(ct);
                return new DesfireEncodingResult(run, deviceFailure);
            }

            run.Fail(EncodingRunStatus.DeviceUnavailable, "Hardware device is unavailable or not ready.", "[]", timeProvider.GetUtcNow());
            await desfireDb.SaveChangesAsync(ct);
            return new DesfireEncodingResult(run, deviceFailure);
        }

        DesfireDeviceLeaseResult leaseResult = await leaseStore.TryAcquireAsync(agentId, deviceId, run.Id, LeaseDuration, ct);
        if (leaseResult == DesfireDeviceLeaseResult.Busy)
        {
            if (requeueOnTransientFailure)
            {
                run.Requeue("Hardware device is busy.", timeProvider.GetUtcNow());
                await desfireDb.SaveChangesAsync(ct);
                return new DesfireEncodingResult(run, Results.Problem("Hardware device is busy.", statusCode: StatusCodes.Status409Conflict));
            }

            run.Fail(EncodingRunStatus.DeviceUnavailable, "Hardware device is busy.", "[]", timeProvider.GetUtcNow());
            await desfireDb.SaveChangesAsync(ct);
            return new DesfireEncodingResult(run, Results.Problem("Hardware device is busy.", statusCode: StatusCodes.Status409Conflict));
        }

        try
        {
            run.Start(agentId, deviceId, timeProvider.GetUtcNow());
            await desfireDb.SaveChangesAsync(ct);

            Transformation? transformation = await desfireDb.Transformations.SingleOrDefaultAsync(candidate => candidate.Id == run.TransformationId, ct);
            if (transformation is null)
                throw new InvalidOperationException("Transformation does not exist.");

            TemplateSpecification current = await ResolveSourceAsync(transformation, ct);
            TemplateSpecification target = await planner.ResolveLatestDesignAsync(transformation.ToChipDesignName, ct);
            ExecutionPlan plan = await ChipDesignTransformer.CreatePlan(keyGroupResolver, current, target, readUid: true);
            if (plan.Errors.Count > 0)
            {
                string error = string.Join("; ", plan.Errors.Select(scriptError => scriptError.Message));
                run.Fail(EncodingRunStatus.Failed, error, "[]", timeProvider.GetUtcNow());
                await desfireDb.SaveChangesAsync(ct);
                return new DesfireEncodingResult(run, Results.Problem(error, statusCode: StatusCodes.Status409Conflict));
            }

            IReadOnlyList<TransformationVariableConfigRequest> variableConfigs = EnsureVariableConfigs(plan.RequiredProviders, JsonSerializer.Deserialize<TransformationVariableConfigRequest[]>(run.VariableConfigJson, DesfireJson.Options) ?? []);
            ResolvedDesfireVariables resolvedVariables = await variableResolver.ResolveAsync(plan.RequiredProviders, variableConfigs, run.InputJson, ct);
            List<CommandAuditItem> audit = [];
            PostHardwareCommandResultRequest presentResult = await ExecuteHardwareCommandAsync(agentId, deviceId, HardwareCapabilities.CardPresent, payload: null, ct);
            AddHardwareAuditItem(audit, HardwareCapabilities.CardPresent, presentResult);
            if (presentResult.Status != HardwareOperationStatus.Succeeded)
            {
                run.Fail(EncodingRunStatus.Failed, presentResult.Error?.Message ?? "Card present operation failed.", JsonSerializer.Serialize(audit, DesfireJson.Options), timeProvider.GetUtcNow());
                await desfireDb.SaveChangesAsync(ct);
                return new DesfireEncodingResult(run, Results.Problem(run.ErrorMessage, statusCode: StatusCodes.Status409Conflict));
            }

            plan.OnCommandExecuted += (_, args) => audit.Add(new CommandAuditItem(args.Index, args.Operation.ToString() ?? args.Operation.GetType().Name, args.Response.IsSuccess, args.Response.StatusCode.ToString(), args.CardUid));
            using HardwareRfidEncoder encoder = new(commandStore, connectionManager, new HardwareDeviceRef(agentId, deviceId));
            ExecutedEncodingPlan executed = await plan.Execute(logger, resolvedVariables.Variables, encoder, ct);
            string auditJson = JsonSerializer.Serialize(audit, DesfireJson.Options);

            if (!executed.IsSuccess)
            {
                PostHardwareCommandResultRequest ejectAfterFailure = await ExecuteHardwareCommandAsync(agentId, deviceId, HardwareCapabilities.CardEject, payload: null, ct);
                AddHardwareAuditItem(audit, HardwareCapabilities.CardEject, ejectAfterFailure);
                auditJson = JsonSerializer.Serialize(audit, DesfireJson.Options);
                string errorMessage = executed.ErrorMessage ?? "Encoding failed.";
                if (ejectAfterFailure.Status != HardwareOperationStatus.Succeeded)
                    errorMessage = $"{errorMessage} Card eject failed: {ejectAfterFailure.Error?.Message ?? ejectAfterFailure.Status.ToString()}.";

                run.Fail(EncodingRunStatus.Failed, errorMessage, auditJson, timeProvider.GetUtcNow());
                await desfireDb.SaveChangesAsync(ct);
                return new DesfireEncodingResult(run, Results.Problem(run.ErrorMessage, statusCode: StatusCodes.Status409Conflict));
            }

            PostHardwareCommandResultRequest ejectResult = await ExecuteHardwareCommandAsync(agentId, deviceId, HardwareCapabilities.CardEject, payload: null, ct);
            AddHardwareAuditItem(audit, HardwareCapabilities.CardEject, ejectResult);
            auditJson = JsonSerializer.Serialize(audit, DesfireJson.Options);
            if (ejectResult.Status != HardwareOperationStatus.Succeeded)
            {
                run.Fail(EncodingRunStatus.Failed, ejectResult.Error?.Message ?? "Card eject operation failed.", auditJson, timeProvider.GetUtcNow());
                await desfireDb.SaveChangesAsync(ct);
                return new DesfireEncodingResult(run, Results.Problem(run.ErrorMessage, statusCode: StatusCodes.Status409Conflict));
            }

            run.Complete(
                executed.CardUid,
                resolvedVariables.AuditJson,
                JsonSerializer.Serialize(new { operationCount = plan.Operations.Count, requiredVariables = plan.RequiredProviders }, DesfireJson.Options),
                auditJson,
                timeProvider.GetUtcNow());
            await desfireDb.SaveChangesAsync(ct);
            return new DesfireEncodingResult(run, null);
        }
        catch (Exception ex) when (ex is InvalidOperationException or FormatException)
        {
            run.Fail(EncodingRunStatus.Failed, ex.Message, "[]", timeProvider.GetUtcNow());
            await desfireDb.SaveChangesAsync(ct);
            return new DesfireEncodingResult(run, Results.Problem(ex.Message, statusCode: StatusCodes.Status409Conflict));
        }
        finally
        {
            await leaseStore.ReleaseAsync(agentId, deviceId, run.Id, CancellationToken.None);
        }
    }

    public async Task<List<HardwareDevice>> GetReadyEncodingDevicesAsync(CancellationToken ct)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        List<HardwareDevice> devices = await hardwareDb.Devices
            .AsNoTracking()
            .Where(device => device.Enabled && device.State.ToLower() == "ready")
            .ToListAsync(ct);

        List<DesfireDeviceLease> activeLeases = await desfireDb.DeviceLeases
            .AsNoTracking()
            .Where(lease => lease.ReleasedAt == null && lease.ExpiresAt > now)
            .ToListAsync(ct);

        return [.. devices.Where(device => SupportsFullEncodingWorkflow(device.Capabilities)
            && !activeLeases.Any(lease => lease.AgentId == device.AgentId && lease.DeviceId == device.DeviceId))];
    }

    private async Task<TemplateSpecification> ResolveSourceAsync(Transformation transformation, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(transformation.FromChipDesignName))
            return await planner.ResolveLatestDesignAsync(transformation.FromChipDesignName, ct);

        return new TemplateSpecification
        {
            Picc = new PiccSpecification
            {
                Key = new KeySpecification
                {
                    KeyGroup = "_blank_",
                    KeyGroupName = "_blank_",
                    KeySet = 0,
                    Key = 0
                }
            },
            Applications = []
        };
    }

    private async Task<IResult?> ValidateDeviceAsync(string agentId, string deviceId, CancellationToken ct)
    {
        string normalizedAgentId = HardwareAgent.NormalizeId(agentId);
        string normalizedDeviceId = HardwareDevice.NormalizeDeviceId(deviceId);
        HardwareDevice? device = await hardwareDb.Devices.AsNoTracking().SingleOrDefaultAsync(candidate => candidate.AgentId == normalizedAgentId && candidate.DeviceId == normalizedDeviceId, ct);
        if (device is null)
            return Results.NotFound();

        if (!device.Enabled)
            return Results.Problem("Hardware device is disabled.", statusCode: StatusCodes.Status409Conflict);

        if (!SupportsFullEncodingWorkflow(device.Capabilities))
            return Results.Problem("Hardware device does not support full RFID encoding workflow.", statusCode: StatusCodes.Status409Conflict);

        if (!string.Equals(device.State, "ready", StringComparison.OrdinalIgnoreCase))
            return Results.Problem("Hardware device is not ready.", statusCode: StatusCodes.Status409Conflict);

        return null;
    }

    private static TransformationVariableConfigRequest[] EnsureVariableConfigs(IReadOnlyList<string> requiredVariables, IReadOnlyList<TransformationVariableConfigRequest> variableConfigs)
    {
        Dictionary<string, TransformationVariableConfigRequest> configsByName = variableConfigs
            .Where(config => !string.IsNullOrWhiteSpace(config.Name))
            .GroupBy(config => config.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

        return [.. requiredVariables.Select(variable => configsByName.GetValueOrDefault(variable) ?? new TransformationVariableConfigRequest(variable, TransformationVariableKind.UserProvided, new VariableFormatRequest(DesfireVariableFormatKind.Hex), Field: variable))];
    }

    private async Task<PostHardwareCommandResultRequest> ExecuteHardwareCommandAsync(string agentId, string deviceId, string capability, JsonObject? payload, CancellationToken cancellationToken)
    {
        PendingHardwareCommand command = commandStore.Create(agentId, deviceId, capability, payload, CardWorkflowCommandTimeout);
        connectionManager.NotifyCommandAvailable(agentId, command.CommandId);
        return await commandStore.WaitForResultAsync(command, cancellationToken);
    }

    private static void AddHardwareAuditItem(List<CommandAuditItem> audit, string operation, PostHardwareCommandResultRequest result)
    {
        string? cardNumber = result.Result?["cardNumber"]?.GetValue<string>();
        audit.Add(new CommandAuditItem(audit.Count, operation, result.Status == HardwareOperationStatus.Succeeded, result.Status.ToString(), cardNumber));
    }

    private static bool SupportsFullEncodingWorkflow(IReadOnlyList<string> capabilities) =>
        capabilities.Contains(HardwareCapabilities.CardPresent, StringComparer.OrdinalIgnoreCase)
        && capabilities.Contains(HardwareCapabilities.RfidApduExchange, StringComparer.OrdinalIgnoreCase)
        && capabilities.Contains(HardwareCapabilities.CardEject, StringComparer.OrdinalIgnoreCase);
}

public sealed record DesfireEncodingResult(EncodingRun? Run, IResult? Failure)
{
    public static DesfireEncodingResult NotFound => new(null, Results.NotFound());
}

public sealed record CommandAuditItem(int Index, string Operation, bool Success, string StatusCode, string? CardUid);
