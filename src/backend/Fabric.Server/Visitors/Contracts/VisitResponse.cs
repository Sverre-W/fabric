using Fabric.Server.Visitors.Domain;
using Riok.Mapperly.Abstractions;

namespace Fabric.Server.Visitors.Contracts;

public record VisitResponse(
    Guid Id,
    string Summary,
    OrganizerResponse Organizer,
    VisitStatus Status,
    DateTimeOffset? Start,
    DateTimeOffset? Stop,
    Guid? LocationId,
    IReadOnlyCollection<VisitInvitationResponse> Invitations);

public record VisitInvitationResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Company,
    ParticipantConfirmationStatus ConfirmationStatus,
    Guid VisitorId,
    DateTimeOffset? RejectedAt,
    DateTimeOffset? ConfirmedAt,
    ModeOfTransport? Transport,
    string? LicensePlate,
    DateTimeOffset? ArrivedAt,
    DateTimeOffset? NoShowAt);

public record VisitorResponse(Guid Id, string FirstName, string LastName, string Email, string? Company, string? LicensePlate);

public record VisitConfirmationResponse(
    Guid VisitId,
    Guid InvitationId,
    string Summary,
    VisitStatus Status,
    DateTimeOffset Start,
    DateTimeOffset Stop,
    Guid? LocationId,
    string? LocationLabel,
    OrganizerResponse Organizer,
    VisitConfirmationVisitorResponse Visitor,
    ParticipantConfirmationStatus ConfirmationStatus,
    DateTimeOffset? RejectedAt,
    DateTimeOffset? ConfirmedAt,
    ModeOfTransport? Transport);

public record VisitConfirmationVisitorResponse(
    Guid VisitorId,
    string FirstName,
    string LastName,
    string Email,
    string Company,
    string? LicensePlate);

[Mapper]
public static partial class VisitMapper
{
    public static VisitResponse ToResponse(this Visit visit, Organizer organizer)
    {
        return new VisitResponse(
            visit.Id,
            visit.Summary,
            organizer.ToResponse(),
            visit.Status,
            visit.Start,
            visit.Stop,
            visit.LocationId,
            visit.Invitations.Select(ToResponse).ToArray());
    }

    [MapperIgnoreSource(nameof(Organizer.Active))]
    public static partial OrganizerResponse ToResponse(this Organizer organizer);

    public static partial VisitorResponse ToResponse(this Visitor visitor);

    public static partial VisitInvitationResponse ToResponse(this VisitInvitation invitation);

    public static VisitConfirmationResponse ToConfirmationResponse(this Visit visit, VisitInvitation invitation, Organizer organizer, string? locationLabel)
    {
        return new VisitConfirmationResponse(
            visit.Id,
            invitation.Id,
            visit.Summary,
            visit.Status,
            visit.Start,
            visit.Stop,
            visit.LocationId,
            locationLabel,
            organizer.ToResponse(),
            new VisitConfirmationVisitorResponse(
                invitation.VisitorId,
                invitation.FirstName,
                invitation.LastName,
                invitation.Email,
                invitation.Company,
                invitation.LicensePlate),
            invitation.ConfirmationStatus,
            invitation.RejectedAt,
            invitation.ConfirmedAt,
            invitation.Transport);
    }
}
