using Fabric.Server.Core;
using Fabric.Server.Reception.Application;
using Fabric.Server.Reception.Domain;
using Fabric.Server.Visitors.Domain;
using Fabric.Server.Visitors.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public enum SagaStepResult
{
    Continue,
    Retry,
    Fail,
}

public class VisitorPreOnboardingSagaService(SagasDbContext db, VisitorsDbContext visitorsDb,
        ReceptionService receptionService,
        TimeProvider timeProvider)
{

    private static readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(10);

    public async Task<VisitorPreOnboardingSaga> StartAsync(Guid visitId, Guid invitationId, DateTimeOffset expiresAt, CancellationToken cancellationToken = default)
    {
        VisitorPreOnboardingState[] terminalStates = [VisitorPreOnboardingState.Cancelled, VisitorPreOnboardingState.Expired];
        bool existing = await db.VisitorPreOnboardingSagas
            .AnyAsync(x => x.VisitId == visitId && x.InvitationId == invitationId && !terminalStates.Contains(x.State), cancellationToken);

        if (existing)
            throw new InvalidOperationException($"Saga already exists for visit {visitId} and invitation {invitationId}");

        var saga = new VisitorPreOnboardingSaga
        {
            Id = Guid.NewGuid(),
            VisitId = visitId,
            InvitationId = invitationId,
            CreatedAt = timeProvider.GetUtcNow(),
            ExpiresAt = expiresAt,
            RetryCount = 0,
            State = VisitorPreOnboardingState.RegisteringArrival,
        };

        db.VisitorPreOnboardingSagas.Add(saga);
        await db.SaveChangesAsync(cancellationToken);
        return saga;
    }

    public async Task ConfirmAsync(Visitor visitor, Guid visitId, Guid invitationId, CancellationToken cancellationToken = default)
    {
        VisitorPreOnboardingSaga saga = await db.VisitorPreOnboardingSagas
        .Where(x => x.VisitId == visitId && x.InvitationId == invitationId)
        .Where(x => x.State == VisitorPreOnboardingState.AwaitingConfirmation)
        .SingleAsync(cancellationToken);

        if (saga.ArrivalId.HasValue)
            _ = await receptionService.ConfirmVisitor(visitor.Id, visitor.FirstName, visitor.LastName, visitor.Company, saga.ArrivalId.Value, cancellationToken);

        saga.State = VisitorPreOnboardingState.Confirmed;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(Guid visitId, Guid invitationId, CancellationToken cancellationToken = default)
    {
        VisitorPreOnboardingSaga saga = await db.VisitorPreOnboardingSagas
        .Where(x => x.VisitId == visitId && x.InvitationId == invitationId)
        .Where(x => x.State == VisitorPreOnboardingState.AwaitingConfirmation)
        .SingleAsync(cancellationToken);

        if (saga.ArrivalId.HasValue)
            _ = await receptionService.RejectConfirmationVisitor(saga.ArrivalId.Value, cancellationToken);

        saga.State = VisitorPreOnboardingState.Rejected;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task VisitRescheduled(Guid visitId, DateTimeOffset startDate, CancellationToken cancellationToken = default)
    {
        List<VisitorPreOnboardingSaga> sagas = await db.VisitorPreOnboardingSagas
      .Where(x => x.VisitId == visitId)
      .ToListAsync(cancellationToken);

        foreach (VisitorPreOnboardingSaga? saga in sagas)
        {
            saga.ExpiresAt = startDate;

            if (saga.ArrivalId.HasValue)
                _ = await receptionService.Reschedule(saga.ArrivalId.Value, startDate, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

    }

    public async Task CancelForVisitAsync(Guid visitId, CancellationToken cancellationToken = default)
    {
        List<VisitorPreOnboardingSaga> sagas = await db.VisitorPreOnboardingSagas
        .Where(x => x.VisitId == visitId)
        .Where(x => x.State != VisitorPreOnboardingState.Cancelled
                 && x.State != VisitorPreOnboardingState.Expired
                 && x.State != VisitorPreOnboardingState.Confirmed
                 && x.State != VisitorPreOnboardingState.Rejected)
        .ToListAsync(cancellationToken);

        foreach (VisitorPreOnboardingSaga? saga in sagas)
        {
            saga.State = VisitorPreOnboardingState.Cancelling;
            saga.RetryCount = 0;
            saga.NextRetryAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RetryAsync(Guid sagaId, CancellationToken cancellationToken = default)
    {
        VisitorPreOnboardingSaga saga = await db.VisitorPreOnboardingSagas
        .Where(x => x.Id == sagaId)
        .Where(x => x.State == VisitorPreOnboardingState.Expired)
        .SingleAsync(cancellationToken);

        if (timeProvider.GetUtcNow() > saga.ExpiresAt)
            throw new InvalidOperationException($"Saga {sagaId} has expired and cannot be retried (visit date has passed).");

        saga.RetryCount = 0;
        saga.NextRetryAt = null;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VisitorPreOnboardingSaga>> GetRetryableAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.VisitorPreOnboardingSagas
            .Where(x => x.State == VisitorPreOnboardingState.RegisteringArrival
                     || x.State == VisitorPreOnboardingState.GeneratingQr
                     || x.State == VisitorPreOnboardingState.UpdatingArrivalQr
                     || x.State == VisitorPreOnboardingState.SendingInvitation
                     || x.State == VisitorPreOnboardingState.Cancelling)
            .Where(x => x.NextRetryAt == null || x.NextRetryAt <= now)
            .Where(x => x.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ExpirePassedSagasAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        List<VisitorPreOnboardingSaga> expired = await db.VisitorPreOnboardingSagas
        .Where(x => x.State == VisitorPreOnboardingState.RegisteringArrival
                 || x.State == VisitorPreOnboardingState.GeneratingQr
                 || x.State == VisitorPreOnboardingState.UpdatingArrivalQr
                 || x.State == VisitorPreOnboardingState.SendingInvitation
                 || x.State == VisitorPreOnboardingState.AwaitingConfirmation)
        .Where(x => x.ExpiresAt <= now)
        .ToListAsync(cancellationToken);

        foreach (VisitorPreOnboardingSaga? saga in expired)
            saga.State = VisitorPreOnboardingState.Expired;

        await db.SaveChangesAsync(cancellationToken);
        return expired.Count;
    }

    public async Task ProcessAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {
        SagaStepResult result;
        do
        {
            result = await StepAsync(saga, cancellationToken);
        }
        while (result == SagaStepResult.Continue && IsRetryableState(saga.State));
    }

    private static bool IsRetryableState(VisitorPreOnboardingState state) =>
        state is VisitorPreOnboardingState.RegisteringArrival
            or VisitorPreOnboardingState.GeneratingQr
            or VisitorPreOnboardingState.UpdatingArrivalQr
            or VisitorPreOnboardingState.SendingInvitation
            or VisitorPreOnboardingState.Cancelling;

    internal async Task<SagaStepResult> StepAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {
        return saga.State switch
        {
            VisitorPreOnboardingState.RegisteringArrival => await RegisterArrivalAsync(saga, cancellationToken),
            VisitorPreOnboardingState.GeneratingQr => await GenerateQrCodeAsync(saga, cancellationToken),
            VisitorPreOnboardingState.UpdatingArrivalQr => await UpdateArrivalQrAsync(saga, cancellationToken),
            VisitorPreOnboardingState.SendingInvitation => await SendInvitationAsync(saga, cancellationToken),
            VisitorPreOnboardingState.Cancelling => await CancelSagaAsync(saga, cancellationToken),
            _ => SagaStepResult.Continue,
        };
    }

    private async Task<SagaStepResult> RegisterArrivalAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {

        Visit visit = await visitorsDb.Visits
            .Include(x => x.Invitations)
            .SingleAsync(x => x.Id == saga.VisitId, cancellationToken);

        VisitInvitation invitation = visit.Invitations.Single(x => x.Id == saga.InvitationId);

        Result<ExpectedArrival, ReceptionErrors> result = await receptionService.RegisterVisitorArrival(invitation.FirstName, invitation.LastName, invitation.Company, invitation.Id, visit.Start, null, visit.LocationId, cancellationToken);


        if (result.IsSuccess(out ExpectedArrival? arrival))
        {
            saga.ArrivalId = arrival.Id;
            saga.State = VisitorPreOnboardingState.GeneratingQr;
            saga.RetryCount = 0;
            saga.NextRetryAt = null;
        }

        if (result.IsFailure(out ReceptionErrors failure))
        {
            saga.ArrivalId = null;
            saga.RetryCount++;
            saga.State = VisitorPreOnboardingState.RegisteringArrival;
            saga.NextRetryAt = timeProvider.GetUtcNow().Add(_retryInterval);
        }

        await db.SaveChangesAsync(cancellationToken);
        return saga.NextRetryAt == null ? SagaStepResult.Continue : SagaStepResult.Retry;
    }

    private async Task<SagaStepResult> GenerateQrCodeAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {
        // TODO: Call access control system to generate QR — may fail
        saga.QrCode = Guid.NewGuid().ToString();
        saga.State = VisitorPreOnboardingState.UpdatingArrivalQr;
        saga.RetryCount = 0;
        saga.NextRetryAt = null;
        await db.SaveChangesAsync(cancellationToken);
        return SagaStepResult.Continue;
    }

    private async Task<SagaStepResult> UpdateArrivalQrAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(saga.QrCode) && saga.ArrivalId.HasValue)
        {
            Result<ReceptionErrors> result = await receptionService.UpdateArrivalCode(saga.ArrivalId.Value, saga.QrCode, cancellationToken);
            if (result.IsSuccess(out _))
            {
                saga.State = VisitorPreOnboardingState.SendingInvitation;
                saga.RetryCount = 0;
                saga.NextRetryAt = null;
                await db.SaveChangesAsync(cancellationToken);

                return SagaStepResult.Continue;
            }
        }

        saga.State = VisitorPreOnboardingState.UpdatingArrivalQr;
        saga.RetryCount++;
        saga.NextRetryAt = timeProvider.GetUtcNow().Add(_retryInterval);
        await db.SaveChangesAsync(cancellationToken);
        return SagaStepResult.Retry;
    }

    private async Task<SagaStepResult> SendInvitationAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {
        Visit? visit = await visitorsDb.Visits
        .Include(x => x.Invitations)
        .SingleOrDefaultAsync(x => x.Id == saga.VisitId, cancellationToken);

        if (visit is null)
            return ScheduleRetry(saga);

        VisitInvitation? invitation = visit.Invitations.FirstOrDefault(x => x.Id == saga.InvitationId);
        if (invitation is null)
            return ScheduleRetry(saga);

        // TODO: Send actual email notification

        saga.State = VisitorPreOnboardingState.AwaitingConfirmation;
        saga.RetryCount = 0;
        saga.NextRetryAt = null;
        await db.SaveChangesAsync(cancellationToken);
        return SagaStepResult.Continue;
    }

    private async Task<SagaStepResult> CancelSagaAsync(VisitorPreOnboardingSaga saga, CancellationToken cancellationToken)
    {
        if (saga.ArrivalId.HasValue)
        {
            _ = await receptionService.Cancel(saga.ArrivalId.Value, cancellationToken);
        }

        if (saga.QrCode is not null)
        {
            // TODO: Cancel QR at access control system
            await Task.Delay(100, cancellationToken);
        }

        saga.State = VisitorPreOnboardingState.Cancelled;
        saga.RetryCount = 0;
        saga.NextRetryAt = null;
        await db.SaveChangesAsync(cancellationToken);
        return SagaStepResult.Continue;
    }

    private SagaStepResult ScheduleRetry(VisitorPreOnboardingSaga saga)
    {
        saga.RetryCount++;
        var delay = TimeSpan.FromMinutes(5 * Math.Pow(2, saga.RetryCount - 1));
        var capped = TimeSpan.FromMinutes(Math.Min(delay.TotalMinutes, 60));
        saga.NextRetryAt = timeProvider.GetUtcNow().Add(capped);
        return SagaStepResult.Retry;
    }
}
