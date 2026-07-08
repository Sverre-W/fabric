using System.Text.Json;
using Fabric.Server.Automation.Kiosk;
using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Kiosk;
using Fabric.Server.Kiosk.Application;
using Fabric.Server.Kiosk.Domain;
using Fabric.Server.Kiosk.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Sagas.Kiosk;

public sealed class KioskSagaService(
    SagasDbContext db,
    KioskDbContext kioskDb,
    KioskInstructionService instructionService,
    KioskWorkflowResumer workflowResumer,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(30);

    public async Task StartAsync(Guid sessionId, string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        bool exists = await db.KioskSagas.AnyAsync(x => x.SessionId == sessionId, cancellationToken);
        if (exists)
            return;

        db.KioskSagas.Add(new KioskSaga
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            WorkflowInstanceId = workflowInstanceId,
            State = KioskSagaState.Active,
            CreatedAt = timeProvider.GetUtcNow(),
            UpdatedAt = timeProvider.GetUtcNow(),
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CleanupAsync(string workflowInstanceId, CancellationToken cancellationToken = default)
    {
        KioskSaga? saga = await db.KioskSagas.SingleOrDefaultAsync(x => x.WorkflowInstanceId == workflowInstanceId, cancellationToken);
        if (saga is null)
            return;

        await db.KioskSagaEvents.Where(x => x.SagaId == saga.Id).ExecuteDeleteAsync(cancellationToken);
        db.KioskSagas.Remove(saga);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<KioskInstructionBookmark> ScheduleInstructionAsync(string workflowInstanceId, KioskInstructionDefinition definition, CancellationToken cancellationToken)
    {
        KioskSaga saga = await db.KioskSagas.SingleAsync(x => x.WorkflowInstanceId == workflowInstanceId, cancellationToken);
        KioskInstructionBookmark bookmark = await instructionService.ShowInstructionAsync(saga.SessionId, definition, cancellationToken);
        saga.CurrentInstructionId = bookmark.InstructionId;
        saga.CurrentInstructionKind = bookmark.Kind;
        saga.State = KioskSagaState.InstructionScheduled;
        saga.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
        return bookmark;
    }

    public async Task<IReadOnlyList<KioskSagaEventWorkItem>> GetDueEventWorkItemsAsync(int take, CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        return await db.KioskSagaEvents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.ProcessedAt == null)
            .Where(x => x.NextRetryAt == null || x.NextRetryAt <= now)
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .Select(x => new KioskSagaEventWorkItem(EF.Property<string>(x, TenantDbContext.TenantIdPropertyName), x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task ProcessEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        KioskSagaEvent? sagaEvent = await db.KioskSagaEvents.SingleOrDefaultAsync(x => x.Id == eventId && x.ProcessedAt == null, cancellationToken);
        if (sagaEvent is null)
            return;

        try
        {
            KioskSaga saga = await db.KioskSagas.SingleAsync(x => x.Id == sagaEvent.SagaId, cancellationToken);
            KioskSession session = await kioskDb.Sessions.SingleAsync(x => x.Id == saga.SessionId, cancellationToken);

            switch (sagaEvent.Type)
            {
                case KioskSagaEventType.InstructionCompleted:
                    await workflowResumer.ResumeInstructionAsync(session, sagaEvent.InstructionId, sagaEvent.InstructionKind, DeserializeResult(sagaEvent.InstructionKind, sagaEvent.ResultJson), cancellationToken);
                    saga.State = KioskSagaState.Active;
                    break;
                case KioskSagaEventType.InstructionCancelled:
                    await workflowResumer.CancelInstructionAsync(session, sagaEvent.InstructionId, sagaEvent.InstructionKind, cancellationToken);
                    saga.State = KioskSagaState.Cancelled;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported kiosk saga event type '{sagaEvent.Type}'.");
            }

            saga.CurrentInstructionId = null;
            saga.CurrentInstructionKind = null;
            saga.UpdatedAt = timeProvider.GetUtcNow();
            sagaEvent.MarkProcessed(timeProvider.GetUtcNow());
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            sagaEvent.ScheduleRetry(timeProvider.GetUtcNow().Add(RetryInterval), exception.Message);
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private static KioskInstructionResult DeserializeResult(KioskInstructionActivityKind kind, string? resultJson) => kind switch
    {
        KioskInstructionActivityKind.Choice when resultJson is not null => JsonSerializer.Deserialize(resultJson, KioskJsonSerializerContext.Default.KioskChoiceInstructionResult) ?? throw new InvalidOperationException("Choice instruction result is invalid."),
        KioskInstructionActivityKind.Form when resultJson is not null => JsonSerializer.Deserialize(resultJson, KioskJsonSerializerContext.Default.KioskFormInstructionResult) ?? throw new InvalidOperationException("Form instruction result is invalid."),
        _ => throw new InvalidOperationException($"Unsupported kiosk instruction result kind '{kind}'.")
    };
}
