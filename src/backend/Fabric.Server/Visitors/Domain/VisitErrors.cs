namespace Fabric.Server.Visitors.Domain;

public enum VisitErrors
{
    VisitNotFound,
    OrganizerNotFound,
    LicensePlateRequired,
    InvalidStatus,
    Cancelled,
    Completed,
    DuplicateInvitationEmail,
    InvitationNotFound,
    InvitationAlreadyResponded,
    AlreadyCancelled,
    StartMustBeBeforeStop,
    StopMustBeFuture
}
