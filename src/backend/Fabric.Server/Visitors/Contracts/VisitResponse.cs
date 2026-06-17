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
    Guid? VisitorId,
    DateTimeOffset? RejectedAt,
    DateTimeOffset? ConfirmedAt,
    ModeOfTransport? Transport,
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

    public static partial VisitInvitationResponse ToResponse(this VisitInvitation invitation);
}
