using Fabric.Server.Core;
using Fabric.Server.Reception.Domain;

namespace Fabric.Server.Reception.Contracts;

public record ListArrivalsRequest : BaseListRequest
{
    public ArrivalType? Type { get; set; }
    public OnboardingStatus? Status { get; set; }
    public bool? CheckedIn { get; set; }
    public Guid? LocationId { get; set; }
}

public record RegisterVisitorArrivalRequest(
    Guid VisitorId,
    Guid InvitationId,
    DateTimeOffset ExpectedArrivalTime,
    DateTimeOffset ExpectedOffboardTime,
    string? ArrivalCode,
    Guid LocationId
);

public record RegisterContractorArrivalRequest(
    Guid ContractorId,
    Guid JobAssignmentId,
    DateTimeOffset ExpectedArrivalTime,
    DateTimeOffset ExpectedOffboardTime,
    string? ArrivalCode,
    Guid LocationId
);

public record OnboardArrivalRequest();

public record OnboardArrivalFromKioskRequest(
    byte[]? FacePicture,
    IdentityVerificationCaptureRequest? IdentityVerification
);

public record ConfirmVisitorRequest(bool Confirmed);

public record IdentityVerificationCaptureRequest(
    IdentityVerificationMethod Method,
    byte[] Content
);

public record CheckInDocumentDto(
    string Name,
    CheckInDocumentType DocumentType,
    byte[] Content
);
