using System.Text.Json;
using Fabric.Hardware.Desfire.Encoding.Models;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Server.Desfire.Application;
using Fabric.Server.Desfire.Domain;

namespace Fabric.Server.Desfire.Contracts;

public sealed record ChipDesignResponse(Guid Id, string Name, int Version, string? Description, TemplateSpecification Specification, DateTimeOffset CreatedAt);

public sealed record CreateChipDesignRequest(string Name, int? Version, string? Description, TemplateSpecification Specification);

public sealed record UpdateChipDesignRequest(string Name, int Version, string? Description, TemplateSpecification Specification);

public sealed record TransformationResponse(Guid Id, string Name, string? FromChipDesignName, bool FromBlank, string ToChipDesignName, bool AlwaysReadUid, IReadOnlyList<string> RequiredVariables, IReadOnlyList<string> RequiredKeyGroups, IReadOnlyList<TransformationVariableConfigRequest> Variables, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record CreateTransformationRequest(string Name, string? FromChipDesignName, bool FromBlank, string ToChipDesignName, IReadOnlyList<TransformationVariableConfigRequest> Variables);

public sealed record UpdateTransformationRequest(string Name, string? FromChipDesignName, bool FromBlank, string ToChipDesignName, IReadOnlyList<TransformationVariableConfigRequest> Variables);

public sealed record TransformationPlanResponse(IReadOnlyList<string> RequiredVariables, IReadOnlyList<string> RequiredKeyGroups, IReadOnlyList<string> Errors, int OperationCount, IReadOnlyList<TransformationPlanOperationResponse> Operations);

public sealed record TransformationPlanOperationResponse(int Order, string Type, string Description);

public sealed record TransformationVariableConfigRequest(string Name, TransformationVariableKind Kind, VariableFormatRequest Format, string? Field = null, SystemVariableProviderKind? SystemProvider = null, string? Value = null, string? SequenceName = null, long? InitialValue = null, Guid? SystemProviderId = null);

public sealed record SystemProviderResponse(Guid Id, string Name, SystemVariableProviderKind ProviderType, string? FixedValue, long? InitialValue, long? CurrentValue, DateTimeOffset CreatedAt);

public sealed record CreateSystemProviderRequest(string Name, SystemVariableProviderKind ProviderType, string? FixedValue, long? InitialValue);

public sealed record EncoderResponse(Guid Id, string Name, string AgentId, string DeviceId, bool SupportsEncoding, bool SupportsPrinting, bool Enabled, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record CreateEncoderRequest(string Name, string AgentId, string DeviceId, bool Enabled = true);

public sealed record UpdateEncoderRequest(string Name, string AgentId, string DeviceId, bool Enabled = true);

public sealed record KeyDiversificationStrategyResponse(Guid Id, string Name, KeyDiversificationAlgorithm Algorithm, IReadOnlyList<DiversificationInput> Inputs, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record CreateKeyDiversificationStrategyRequest(string Name, KeyDiversificationAlgorithm Algorithm, IReadOnlyList<DiversificationInput> Inputs);

public sealed record UpdateKeyDiversificationStrategyRequest(string Name, KeyDiversificationAlgorithm Algorithm, IReadOnlyList<DiversificationInput> Inputs);

public sealed record KeyGroupResponse(Guid Id, string Name, KeyType KeyType, bool Locked, Guid? DiversificationStrategyId, IReadOnlyList<KeyGroupKeySetResponse> KeySets, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record KeyGroupKeySetResponse(int KeySetId, IReadOnlyList<KeyGroupKeyResponse> Keys);

public sealed record KeyGroupKeyResponse(int KeyId, bool IsDiversified, string? Value);

public sealed record CreateKeyGroupRequest(string Name, KeyType KeyType, int NumberOfKeySets, int NumberOfKeys);

public sealed record UpdateKeyGroupRequest(string Name, Guid? DiversificationStrategyId, IReadOnlyList<KeyGroupKeySetRequest> KeySets);

public sealed record KeyGroupKeySetRequest(int KeySetId, IReadOnlyList<KeyGroupKeyRequest> Keys);

public sealed record KeyGroupKeyRequest(int KeyId, string Value, bool IsDiversified);

public sealed record EncodingBatchResponse(Guid Id, string Name, Guid? EncoderId, Guid TransformationId, EncodingBatchStatus Status, JsonElement OriginalInput, JsonElement NormalizedRows, int TotalRuns, int PendingRuns, int RunningRuns, int SucceededRuns, int FailedRuns, int CancelledRuns, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record CreateEncodingBatchRequest(string Name, Guid EncoderId, Guid TransformationId, JsonElement OriginalInput, JsonElement NormalizedRows, string? RequestedAgentId, string? RequestedDeviceId, int Priority = 0);

public sealed record EncodingBatchRunSummary(Guid BatchId, int TotalRuns, int PendingRuns, int RunningRuns, int SucceededRuns, int FailedRuns, int CancelledRuns)
{
    public EncodingBatchStatus Status => TotalRuns switch
    {
        0 => EncodingBatchStatus.Pending,
        _ when RunningRuns > 0 => EncodingBatchStatus.Running,
        _ when PendingRuns > 0 => SucceededRuns > 0 || FailedRuns > 0 || CancelledRuns > 0 ? EncodingBatchStatus.Running : EncodingBatchStatus.Pending,
        _ when FailedRuns > 0 => EncodingBatchStatus.Failed,
        _ when CancelledRuns > 0 && SucceededRuns == 0 => EncodingBatchStatus.Cancelled,
        _ => EncodingBatchStatus.Completed
    };
}

public sealed record EncodingRunResponse(Guid Id, Guid TransformationId, Guid? BatchId, Guid? EncoderId, EncodingRunKind Kind, string? Source, EncodingRunStatus Status, JsonElement Input, JsonElement ResolvedVariables, JsonElement PlanSummary, JsonElement CommandAudit, string? CardUid, string? HardwareAgentId, string? DeviceId, string? ErrorMessage, DateTimeOffset RequestedAt, DateTimeOffset? StartedAt, DateTimeOffset? CompletedAt);

public sealed record CreateAdHocEncodingRequest(Guid TransformationId, string? AgentId, string? DeviceId, JsonElement UserVariables, AdHocEncodingMode Mode = AdHocEncodingMode.Sync, int Priority = 0, string? Source = null, Guid? KioskSessionId = null);

public sealed record EncodingVariableRequest(string Name, VariableProviderRequest Provider, VariableFormatRequest Format);

public sealed record VariableProviderRequest(DesfireVariableProviderKind Type, string? Field = null, string? Value = null, string? SequenceName = null, long? InitialValue = null, Guid? SystemProviderId = null);

public sealed record VariableFormatRequest(DesfireVariableFormatKind Type, int? Length = null, string? Encoding = null, GenericWiegandFormatRequest? Wiegand = null);

public sealed record GenericWiegandFormatRequest(int BitLength, IReadOnlyList<WiegandFieldRequest> Fields, IReadOnlyList<WiegandParityRequest> Parity, string Output = "hex");

public sealed record WiegandFieldRequest(string Name, int Offset, int Length, WiegandFieldSourceKind Source, string? Field = null, string? Value = null, string? SequenceName = null, long? InitialValue = null);

public sealed record WiegandParityRequest(int Offset, WiegandParityKind Kind, int CoversOffset, int CoversLength);

public static class DesfireMapper
{
    public static ChipDesignResponse ToResponse(this ChipDesign design) => new(
        design.Id,
        design.Name,
        design.Version,
        design.Description,
        JsonSerializer.Deserialize<TemplateSpecification>(design.SpecificationJson, DesfireJson.Options)!,
        design.CreatedAt);

    public static TransformationResponse ToResponse(this Transformation transformation) => new(
        transformation.Id,
        transformation.Name,
        transformation.FromChipDesignName,
        transformation.FromBlank,
        transformation.ToChipDesignName,
        transformation.AlwaysReadUid,
        JsonSerializer.Deserialize<string[]>(transformation.RequiredVariablesJson, DesfireJson.Options) ?? [],
        JsonSerializer.Deserialize<string[]>(transformation.RequiredKeyGroupsJson, DesfireJson.Options) ?? [],
        JsonSerializer.Deserialize<TransformationVariableConfigRequest[]>(transformation.VariableConfigsJson, DesfireJson.Options) ?? [],
        transformation.CreatedAt,
        transformation.UpdatedAt);

    public static KeyDiversificationStrategyResponse ToResponse(this KeyDiversificationStrategyEntity strategy) => new(
        strategy.Id,
        strategy.Name,
        strategy.Algorithm,
        JsonSerializer.Deserialize<DiversificationInput[]>(strategy.InputsJson, DesfireJson.Options) ?? [],
        strategy.CreatedAt,
        strategy.UpdatedAt);

    public static KeyGroupResponse ToResponse(this KeyGroup group, IDesfireKeyProtector? keyProtector = null) => new(
        group.Id,
        group.Name,
        group.KeyType,
        group.Locked,
        group.DiversificationStrategyId,
        group.KeySets.OrderBy(keySet => keySet.KeySetId).Select(keySet => new KeyGroupKeySetResponse(
            keySet.KeySetId,
            keySet.Keys.OrderBy(key => key.KeyId).Select(key => new KeyGroupKeyResponse(
                key.KeyId,
                key.IsDiversified,
                keyProtector is null || group.Locked ? null : keyProtector.Unprotect(key.ProtectedValue))).ToArray())).ToArray(),
        group.CreatedAt,
        group.UpdatedAt);

    public static SystemProviderResponse ToResponse(this DesfireSystemProvider provider) => new(
        provider.Id,
        provider.Name,
        provider.ProviderType,
        provider.FixedValue,
        provider.InitialValue,
        provider.CurrentValue,
        provider.CreatedAt);

    public static EncoderResponse ToResponse(this DesfireEncoder encoder) => new(
        encoder.Id,
        encoder.Name,
        encoder.AgentId,
        encoder.DeviceId,
        encoder.SupportsEncoding,
        encoder.SupportsPrinting,
        encoder.Enabled,
        encoder.CreatedAt,
        encoder.UpdatedAt);

    public static EncodingBatchResponse ToResponse(this EncodingBatch batch, EncodingBatchRunSummary? summary = null) => new(
        batch.Id,
        batch.Name,
        batch.EncoderId,
        batch.TransformationId,
        summary?.Status ?? batch.Status,
        JsonSerializer.Deserialize<JsonElement>(batch.OriginalInputJson, DesfireJson.Options),
        JsonSerializer.Deserialize<JsonElement>(batch.NormalizedRowsJson, DesfireJson.Options),
        summary?.TotalRuns ?? 0,
        summary?.PendingRuns ?? 0,
        summary?.RunningRuns ?? 0,
        summary?.SucceededRuns ?? 0,
        summary?.FailedRuns ?? 0,
        summary?.CancelledRuns ?? 0,
        batch.CreatedAt,
        batch.UpdatedAt);

    public static EncodingRunResponse ToResponse(this EncodingRun run) => new(
        run.Id,
        run.TransformationId,
        run.BatchId,
        run.EncoderId,
        run.Kind,
        run.Source,
        run.Status,
        JsonSerializer.Deserialize<JsonElement>(run.InputJson, DesfireJson.Options),
        JsonSerializer.Deserialize<JsonElement>(run.ResolvedVariablesJson, DesfireJson.Options),
        JsonSerializer.Deserialize<JsonElement>(run.PlanSummaryJson, DesfireJson.Options),
        JsonSerializer.Deserialize<JsonElement>(run.CommandAuditJson, DesfireJson.Options),
        run.CardUid,
        run.HardwareAgentId,
        run.DeviceId,
        run.ErrorMessage,
        run.RequestedAt,
        run.StartedAt,
        run.CompletedAt);
}
