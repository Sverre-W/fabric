using Fabric.Server.Core;
using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Notifications.Services;
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
        EmailNotificationSender emailNotificationSender,
        IWebHostEnvironment webHostEnvironment,
        TimeProvider timeProvider)
{

    private static readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(10);
    private const string InvitationTemplate = "invitation.html";
    private const string ConfirmationTemplate = "confirmation-to-organizer.html";
    private const string CancellationTemplate = "cancellation.html";
    private const string RescheduleTemplate = "reschedule.html";
    private const string InvitationSubject = "You're invited to a visit";
    private const string ConfirmationSubject = "Visitor confirmed participation";
    private const string CancellationSubject = "Your visit has been cancelled";
    private const string RescheduleSubject = "Your visit has been rescheduled";

    public async Task<VisitorPreOnboardingSagaConfig> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        VisitorPreOnboardingSagaConfig? config = await db.VisitorPreOnboardingSagaConfigs
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return config ?? VisitorPreOnboardingSagaConfig.Default;
    }

    public async Task<VisitorPreOnboardingSagaConfig> UpdateConfigurationAsync(VisitorPreOnboardingSagaConfig config, CancellationToken cancellationToken = default)
    {
        VisitorPreOnboardingSagaConfig? existing = await db.VisitorPreOnboardingSagaConfigs
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            existing = new VisitorPreOnboardingSagaConfig { Id = Guid.NewGuid() };
            db.VisitorPreOnboardingSagaConfigs.Add(existing);
        }

        existing.UseCustomInviteNotification = config.UseCustomInviteNotification;
        existing.CustomInviteNotification = config.CustomInviteNotification;
        existing.QrGenerationMode = config.QrGenerationMode;
        existing.SendConfirmNotificationToOrganizer = config.SendConfirmNotificationToOrganizer;
        existing.UseCustomConfirmNotification = config.UseCustomConfirmNotification;
        existing.CustomConfirmNotification = config.CustomConfirmNotification;
        existing.SendCancellationNotification = config.SendCancellationNotification;
        existing.UseCustomCancellationNotification = config.UseCustomCancellationNotification;
        existing.CustomCancellationNotification = config.CustomCancellationNotification;
        existing.SendRescheduleNotification = config.SendRescheduleNotification;
        existing.UseCustomRescheduleNotification = config.UseCustomRescheduleNotification;
        existing.CustomRescheduleNotification = config.CustomRescheduleNotification;

        await db.SaveChangesAsync(cancellationToken);
        return existing;
    }

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
            _ = await receptionService.ConfirmVisitor(visitor.FirstName, visitor.LastName, visitor.Company, saga.ArrivalId.Value, cancellationToken);

        saga.State = VisitorPreOnboardingState.Confirmed;
        await db.SaveChangesAsync(cancellationToken);

        VisitorPreOnboardingSagaConfig config = await GetConfigurationAsync(cancellationToken);
        if (config.SendConfirmNotificationToOrganizer)
            await SendConfirmationToOrganizerAsync(config, visitId, invitationId, cancellationToken);
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

    public async Task VisitRescheduled(Guid visitId, DateTimeOffset startDate, DateTimeOffset stopDate, CancellationToken cancellationToken = default)
    {
        List<VisitorPreOnboardingSaga> sagas = await db.VisitorPreOnboardingSagas
      .Where(x => x.VisitId == visitId)
      .ToListAsync(cancellationToken);

        foreach (VisitorPreOnboardingSaga? saga in sagas)
        {
            saga.ExpiresAt = startDate;

            if (saga.ArrivalId.HasValue)
                _ = await receptionService.Reschedule(saga.ArrivalId.Value, startDate, stopDate, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        VisitorPreOnboardingSagaConfig config = await GetConfigurationAsync(cancellationToken);
        if (!config.SendRescheduleNotification)
            return;

        foreach (VisitorPreOnboardingSaga saga in sagas)
            await SendVisitorNotificationAsync(saga.VisitId, saga.InvitationId, RescheduleTemplate, config.UseCustomRescheduleNotification, config.CustomRescheduleNotification, RescheduleSubject, cancellationToken);

    }

    public async Task VisitRelocated(Guid visitId, Guid locationId, CancellationToken cancellationToken = default)
    {
        List<VisitorPreOnboardingSaga> sagas = await db.VisitorPreOnboardingSagas
            .Where(x => x.VisitId == visitId)
            .ToListAsync(cancellationToken);

        foreach (VisitorPreOnboardingSaga saga in sagas)
        {
            if (saga.ArrivalId.HasValue)
                _ = await receptionService.Relocate(saga.ArrivalId.Value, locationId, cancellationToken);
        }
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

    public async Task<IReadOnlyList<VisitorPreOnboardingSagaWorkItem>> GetRetryableWorkItemsAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.VisitorPreOnboardingSagas
            .IgnoreQueryFilters()
            .Where(x => x.State == VisitorPreOnboardingState.RegisteringArrival
                     || x.State == VisitorPreOnboardingState.GeneratingQr
                     || x.State == VisitorPreOnboardingState.UpdatingArrivalQr
                     || x.State == VisitorPreOnboardingState.SendingInvitation
                     || x.State == VisitorPreOnboardingState.Cancelling)
            .Where(x => x.NextRetryAt == null || x.NextRetryAt <= now)
            .Where(x => x.ExpiresAt > now)
            .Select(x => new VisitorPreOnboardingSagaWorkItem(
                EF.Property<string>(x, TenantDbContext.TenantIdPropertyName),
                x.Id))
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

    public async Task<int> ExpirePassedSagasForAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();

        return await db.VisitorPreOnboardingSagas
            .IgnoreQueryFilters()
            .Where(x => x.State == VisitorPreOnboardingState.RegisteringArrival
                     || x.State == VisitorPreOnboardingState.GeneratingQr
                     || x.State == VisitorPreOnboardingState.UpdatingArrivalQr
                     || x.State == VisitorPreOnboardingState.SendingInvitation
                     || x.State == VisitorPreOnboardingState.AwaitingConfirmation)
            .Where(x => x.ExpiresAt <= now)
            .ExecuteUpdateAsync(x => x.SetProperty(saga => saga.State, VisitorPreOnboardingState.Expired), cancellationToken);
    }

    public async Task ProcessAsync(Guid sagaId, CancellationToken cancellationToken)
    {
        VisitorPreOnboardingSaga? saga = await db.VisitorPreOnboardingSagas
            .SingleOrDefaultAsync(x => x.Id == sagaId, cancellationToken);

        if (saga is null)
            return;

        await ProcessAsync(saga, cancellationToken);
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

        Result<ExpectedArrival, ReceptionErrors> result = await receptionService.RegisterVisitorArrival(invitation.FirstName, invitation.LastName, invitation.Company, invitation.VisitorId, invitation.Id, visit.Start, visit.Stop, null, visit.LocationId, cancellationToken);


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
        {
            SagaStepResult result = ScheduleRetry(saga);
            await db.SaveChangesAsync(cancellationToken);
            return result;
        }

        VisitInvitation? invitation = visit.Invitations.FirstOrDefault(x => x.Id == saga.InvitationId);
        if (invitation is null)
        {
            SagaStepResult result = ScheduleRetry(saga);
            await db.SaveChangesAsync(cancellationToken);
            return result;
        }

        VisitorPreOnboardingSagaConfig config = await GetConfigurationAsync(cancellationToken);
        NotificationContent notification = await GetNotificationContentAsync(InvitationSubject, InvitationTemplate, config.UseCustomInviteNotification, config.CustomInviteNotification, cancellationToken);
        Result<EmailErrors> emailResult = await emailNotificationSender.SendEmail(
            notification.Subject,
            notification.Body,
            new SagaNotificationModel(visit, invitation),
            [invitation.Email],
            ct: cancellationToken);

        if (emailResult.IsFailure(out _))
        {
            SagaStepResult result = ScheduleRetry(saga);
            await db.SaveChangesAsync(cancellationToken);
            return result;
        }

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

        VisitorPreOnboardingSagaConfig config = await GetConfigurationAsync(cancellationToken);
        if (config.SendCancellationNotification)
        {
            bool sent = await SendVisitorNotificationAsync(saga.VisitId, saga.InvitationId, CancellationTemplate, config.UseCustomCancellationNotification, config.CustomCancellationNotification, CancellationSubject, cancellationToken);
            if (!sent)
            {
                SagaStepResult result = ScheduleRetry(saga);
                await db.SaveChangesAsync(cancellationToken);
                return result;
            }
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

    private async Task SendConfirmationToOrganizerAsync(VisitorPreOnboardingSagaConfig config, Guid visitId, Guid invitationId, CancellationToken cancellationToken)
    {
        Visit? visit = await visitorsDb.Visits
            .Include(x => x.Invitations)
            .SingleOrDefaultAsync(x => x.Id == visitId, cancellationToken);

        if (visit is null)
            return;

        VisitInvitation? invitation = visit.Invitations.FirstOrDefault(x => x.Id == invitationId);
        if (invitation is null)
            return;

        Organizer? organizer = await visitorsDb.Organizers.SingleOrDefaultAsync(x => x.Id == visit.OrganizerId, cancellationToken);
        if (organizer is null)
            return;

        NotificationContent notification = await GetNotificationContentAsync(ConfirmationSubject, ConfirmationTemplate, config.UseCustomConfirmNotification, config.CustomConfirmNotification, cancellationToken);
        _ = await emailNotificationSender.SendEmail(
            notification.Subject,
            notification.Body,
            new SagaNotificationModel(visit, invitation),
            [organizer.Email],
            ct: cancellationToken);
    }

    private async Task<bool> SendVisitorNotificationAsync(
        Guid visitId,
        Guid invitationId,
        string defaultTemplate,
        bool useCustomTemplate,
        CustomNotification? customNotification,
        string subject,
        CancellationToken cancellationToken)
    {
        Visit? visit = await visitorsDb.Visits
            .Include(x => x.Invitations)
            .SingleOrDefaultAsync(x => x.Id == visitId, cancellationToken);

        if (visit is null)
            return false;

        VisitInvitation? invitation = visit.Invitations.FirstOrDefault(x => x.Id == invitationId);
        if (invitation is null)
            return false;

        NotificationContent notification = await GetNotificationContentAsync(subject, defaultTemplate, useCustomTemplate, customNotification, cancellationToken);
        Result<EmailErrors> emailResult = await emailNotificationSender.SendEmail(
            notification.Subject,
            notification.Body,
            new SagaNotificationModel(visit, invitation),
            [invitation.Email],
            ct: cancellationToken);

        return emailResult.IsSuccess(out _);
    }

    private async Task<NotificationContent> GetNotificationContentAsync(string defaultSubject, string defaultTemplate, bool useCustomTemplate, CustomNotification? customNotification, CancellationToken cancellationToken)
    {
        if (useCustomTemplate && customNotification is not null)
            return new NotificationContent(customNotification.Subject, customNotification.Body);

        string path = Path.Combine(webHostEnvironment.ContentRootPath, "Sagas", "VisitorPreOnboarding", "default-templates", defaultTemplate);
        string body = await File.ReadAllTextAsync(path, cancellationToken);
        return new NotificationContent(defaultSubject, body);
    }

    private sealed record NotificationContent(string Subject, string Body);
}
