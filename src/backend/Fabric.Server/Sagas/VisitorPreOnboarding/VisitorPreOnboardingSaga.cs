using Fabric.Server.Visitors.Domain;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public class VisitorPreOnboardingSaga
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid InvitationId { get; set; }
    public Guid? ArrivalId { get; set; }
    public string? QrCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? NextRetryAt { get; set; }
    public int RetryCount { get; set; }
    public VisitorPreOnboardingState State { get; set; }
}

public enum VisitorPreOnboardingState
{
    RegisteringArrival,
    GeneratingQr,
    UpdatingArrivalQr,
    SendingInvitation,
    AwaitingConfirmation,
    Confirmed,
    Rejected,
    Cancelling,
    Cancelled,
    Expired,
}

public enum CredentialGenerationMode
{
    PlatformQr,
    AccessControlQr
}

public class VisitorPreOnboardingSagaConfig
{
    public Guid Id { get; set; }
    public bool UseCustomInviteNotification { get; set; }
    public CustomNotification? CustomInviteNotification { get; set; }
    public CredentialGenerationMode QrGenerationMode { get; set; }
    public bool SendConfirmNotificationToOrganizer { get; set; }
    public bool UseCustomConfirmNotification { get; set; }
    public CustomNotification? CustomConfirmNotification { get; set; }
    public bool SendCancellationNotification { get; set; }
    public bool UseCustomCancellationNotification { get; set; }
    public CustomNotification? CustomCancellationNotification { get; set; }
    public bool SendRescheduleNotification { get; set; }
    public bool UseCustomRescheduleNotification { get; set; }
    public CustomNotification? CustomRescheduleNotification { get; set; }

    public static VisitorPreOnboardingSagaConfig Default => new()
    {
        Id = Guid.Empty,
        UseCustomInviteNotification = false,
        CustomInviteNotification = null,
        QrGenerationMode = CredentialGenerationMode.PlatformQr,
        SendConfirmNotificationToOrganizer = false,
        UseCustomConfirmNotification = false,
        CustomConfirmNotification = null,
        SendCancellationNotification = false,
        UseCustomCancellationNotification = false,
        CustomCancellationNotification = null,
        SendRescheduleNotification = false,
        UseCustomRescheduleNotification = false,
        CustomRescheduleNotification = null,
    };
}

public sealed record CustomNotification
{
    public required string Subject { get; init; }
    public required string Body { get; init; }
}

public record SagaNotificationModel(Visit Visit, VisitInvitation Visitor);
