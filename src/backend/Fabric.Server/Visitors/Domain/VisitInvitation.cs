using Fabric.Server.Core;

namespace Fabric.Server.Visitors.Domain;

public enum ParticipantConfirmationStatus
{
    Tentative,
    Rejected,
    Confirmed
}

public enum ModeOfTransport
{
    Car,
    PublicTransport,
    Bike,
    Walk
}

public sealed class VisitInvitation
{
    private VisitInvitation() { }

    public Guid Id { get; internal set; }
    public string FirstName { get; internal set; } = null!;
    public string LastName { get; internal set; } = null!;
    public string Email { get; internal set; } = null!;

    public string Company { get; internal set; } = null!;

    public ParticipantConfirmationStatus ConfirmationStatus { get; internal set; }

    public Guid VisitorId { get; internal set; }
    public DateTimeOffset? RejectedAt { get; internal set; }
    public DateTimeOffset? ConfirmedAt { get; internal set; }

    public ModeOfTransport? Transport { get; internal set; }
    public string? LicensePlate { get; internal set; }
    public DateTimeOffset? ArrivedAt { get; internal set; }
    public DateTimeOffset? NoShowAt { get; internal set; }

    internal static VisitInvitation Create(Guid id, Guid visitorId, string firstName, string lastName, string email, string company)
    {
        return new VisitInvitation
        {
            Id = id,
            VisitorId = visitorId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Company = company,
            ConfirmationStatus = ParticipantConfirmationStatus.Tentative
        };
    }

    internal Result<VisitErrors> Confirm(
        ModeOfTransport modeOfTransport,
        string? licensePlate,
        DateTimeOffset timestamp)
    {
        if (ConfirmationStatus != ParticipantConfirmationStatus.Tentative)
            return Result.Failure(VisitErrors.InvitationAlreadyResponded);

        if (modeOfTransport == ModeOfTransport.Car && string.IsNullOrWhiteSpace(licensePlate))
            return Result.Failure(VisitErrors.LicensePlateRequired);

        ConfirmationStatus = ParticipantConfirmationStatus.Confirmed;
        ConfirmedAt = timestamp;
        Transport = modeOfTransport;
        LicensePlate = modeOfTransport == ModeOfTransport.Car? licensePlate : null;
        RejectedAt = null;

        return Result.Success<VisitErrors>();
    }

    internal Result<VisitErrors> Reject(DateTimeOffset timestamp)
    {
        if (ConfirmationStatus != ParticipantConfirmationStatus.Tentative)
            return Result.Failure(VisitErrors.InvitationAlreadyResponded);

        ConfirmationStatus = ParticipantConfirmationStatus.Rejected;
        RejectedAt = timestamp;
        ConfirmedAt = null;
        Transport = null;
        LicensePlate = null;
        NoShowAt = null;

        return Result.Success<VisitErrors>();
    }

    internal void MarkArrived(DateTimeOffset timestamp)
    {
        ArrivedAt ??= timestamp;
        NoShowAt = null;
    }

    internal void MarkNoShow(DateTimeOffset timestamp)
    {
        if (ConfirmationStatus == ParticipantConfirmationStatus.Rejected || ArrivedAt.HasValue)
            return;

        NoShowAt ??= timestamp;
    }
}
