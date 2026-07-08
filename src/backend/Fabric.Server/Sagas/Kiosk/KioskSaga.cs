using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Sagas.Kiosk;

public sealed class KioskSaga
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string WorkflowInstanceId { get; set; } = default!;
    public string? CurrentInstructionId { get; set; }
    public KioskInstructionActivityKind? CurrentInstructionKind { get; set; }
    public KioskSagaState State { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class KioskSagaEvent
{
    private KioskSagaEvent() { }

    public Guid Id { get; private set; }
    public Guid SagaId { get; private set; }
    public KioskSagaEventType Type { get; private set; }
    public string InstructionId { get; private set; } = default!;
    public KioskInstructionActivityKind InstructionKind { get; private set; }
    public string? ResultJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? NextRetryAt { get; private set; }
    public int RetryCount { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    public static KioskSagaEvent Create(Guid sagaId, KioskSagaEventType type, string instructionId, KioskInstructionActivityKind instructionKind, string? resultJson, DateTimeOffset createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            SagaId = sagaId,
            Type = type,
            InstructionId = instructionId,
            InstructionKind = instructionKind,
            ResultJson = resultJson,
            CreatedAt = createdAt,
            RetryCount = 0,
        };

    public void MarkProcessed(DateTimeOffset timestamp)
    {
        ProcessedAt = timestamp;
        FailureReason = null;
        NextRetryAt = null;
    }

    public void ScheduleRetry(DateTimeOffset nextRetryAt, string? failureReason)
    {
        RetryCount++;
        NextRetryAt = nextRetryAt;
        FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason;
    }
}

public sealed record KioskSagaEventWorkItem(string TenantId, Guid EventId);

public enum KioskSagaState
{
    Active,
    InstructionScheduled,
    Failed,
    Cancelled,
    Completed
}

public enum KioskSagaEventType
{
    InstructionCompleted,
    InstructionCancelled
}
