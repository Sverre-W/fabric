using Fabric.Server.Reception.Domain;
using Fabric.Server.Visitors.Domain;
using Riok.Mapperly.Abstractions;

namespace Fabric.Server.Reception.Contracts;

public record ReceptionKioskResponse(
    Guid Id,
    string Name,
    Guid LocationId,
    bool Enabled
);

public record CreateReceptionKioskRequest(
    string Name,
    Guid LocationId
);

public record UpdateReceptionKioskRequest(
    string Name,
    Guid LocationId,
    bool Enabled
);

public record ReceptionKioskKeyResponse(
    ReceptionKioskResponse Kiosk,
    string ApiKey
);

public record ReceptionKioskExpectedArrivalResponse(
    Guid Id,
    ArrivalType Type,
    DateTimeOffset ExpectedArrivalTime,
    DateTimeOffset ExpectedOffboardTime,
    string FirstName,
    string LastName,
    string? Company,
    OnboardingStatus Status,
    bool CheckedIn,
    Guid? LocationId,
    ReceptionKioskVisitorDetailsResponse? Visitor,
    ReceptionKioskContractorDetailsResponse? Contractor
);

public record ReceptionKioskVisitorDetailsResponse(
    Guid VisitorId,
    Guid InvitationId,
    string Email,
    ParticipantConfirmationStatus ConfirmationStatus,
    ModeOfTransport? Transport,
    string? LicensePlate,
    ReceptionKioskVisitDetailsResponse? Visit
);

public record ReceptionKioskVisitDetailsResponse(
    Guid Id,
    string Summary,
    VisitStatus Status,
    DateTimeOffset Start,
    DateTimeOffset Stop,
    Guid? LocationId,
    string OrganizerName,
    string OrganizerEmail
);

public record ReceptionKioskContractorDetailsResponse();

[Mapper]
public static partial class ReceptionKioskMapper
{
    [MapperIgnoreSource(nameof(ReceptionKiosk.ApiKeyHash))]
    [MapperIgnoreSource(nameof(ReceptionKiosk.ApiKeySalt))]
    public static partial ReceptionKioskResponse ToResponse(this ReceptionKiosk kiosk);

    public static ReceptionKioskExpectedArrivalResponse ToKioskResponse(
        this ExpectedArrival arrival,
        ReceptionKioskVisitorDetailsResponse? visitor = null,
        ReceptionKioskContractorDetailsResponse? contractor = null) =>
        new(
            arrival.Id,
            arrival.Type,
            arrival.ExpectedArrivalTime,
            arrival.ExpectedOffboardTime,
            arrival.FirstName,
            arrival.LastName,
            arrival.Company,
            arrival.Status,
            arrival.CheckedIn,
            arrival.LocationId,
            visitor,
            contractor);
}
