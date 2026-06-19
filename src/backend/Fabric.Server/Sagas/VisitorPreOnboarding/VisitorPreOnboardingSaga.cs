using System.Text.Json.Serialization;

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

[JsonConverter(typeof(JsonStringEnumConverter))]
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