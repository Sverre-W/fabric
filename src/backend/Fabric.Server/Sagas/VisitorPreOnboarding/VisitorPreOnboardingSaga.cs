using Fabric.Server.Locations.Domain;
using Fabric.Server.Locations.Persistence;
using Fabric.Server.Visitors.Domain;

namespace Fabric.Server.Sagas.VisitorPreOnboarding;

public class VisitorPreOnboardingSaga
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public Guid InvitationId { get; set; }
    public Guid? ArrivalId { get; set; }
    public Guid? AccessPolicyId { get; set; }
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
    public Guid? SystemId { get; set; }
    public Guid? BadgeTypeId { get; set; }
    public bool SendConfirmNotificationToOrganizer { get; set; }
    public bool UseCustomConfirmNotification { get; set; }
    public CustomNotification? CustomConfirmNotification { get; set; }
    public bool SendCancellationNotification { get; set; }
    public bool UseCustomCancellationNotification { get; set; }
    public CustomNotification? CustomCancellationNotification { get; set; }
    public bool SendRescheduleNotification { get; set; }
    public bool UseCustomRescheduleNotification { get; set; }
    public CustomNotification? CustomRescheduleNotification { get; set; }
    public bool SendRelocationNotification { get; set; }
    public bool UseCustomRelocationNotification { get; set; }
    public CustomNotification? CustomRelocationNotification { get; set; }

    public static VisitorPreOnboardingSagaConfig Default => new()
    {
        Id = Guid.Empty,
        UseCustomInviteNotification = false,
        CustomInviteNotification = null,
        QrGenerationMode = CredentialGenerationMode.PlatformQr,
        SystemId = null,
        BadgeTypeId = null,
        SendConfirmNotificationToOrganizer = false,
        UseCustomConfirmNotification = false,
        CustomConfirmNotification = null,
        SendCancellationNotification = false,
        UseCustomCancellationNotification = false,
        CustomCancellationNotification = null,
        SendRescheduleNotification = false,
        UseCustomRescheduleNotification = false,
        CustomRescheduleNotification = null,
        SendRelocationNotification = false,
        UseCustomRelocationNotification = false,
        CustomRelocationNotification = null,
    };
}

public sealed record CustomNotification
{
    public required string Subject { get; init; }
    public required string Body { get; init; }
}

public record SagaNotificationModel(VisitInvitation Visitor, VisitNotificationModel Visit, LocationNotificationModel? Location, string PlatformBaseUrl, string? QrCodeLink);

public record VisitNotificationModel(
    Guid Id,
    string Summary,
    Guid OrganizerId,
    VisitStatus Status,
    DateTimeOffset Start,
    DateTimeOffset Stop,
    Guid? LocationId)
{
    public static VisitNotificationModel FromVisit(Visit visit) => new(
        visit.Id,
        visit.Summary,
        visit.OrganizerId,
        visit.Status,
        visit.Start,
        visit.Stop,
        visit.LocationId);
}

public record LocationNotificationModel(
    Guid Id,
    LocationType Type,
    LocationPartNotificationModel Site,
    LocationPartNotificationModel? Building,
    LocationPartNotificationModel? Room,
    string DisplayName)
{
    public static LocationNotificationModel FromLocation(Location location) => location switch
    {
        Location.SiteLocation siteLocation => new(
            siteLocation.Id,
            LocationType.Site,
            LocationPartNotificationModel.FromSite(siteLocation.Site),
            null,
            null,
            siteLocation.Site.Name),
        Location.BuildingLocation buildingLocation => new(
            buildingLocation.Id,
            LocationType.Building,
            LocationPartNotificationModel.FromSite(buildingLocation.Site),
            LocationPartNotificationModel.FromBuilding(buildingLocation.Building),
            null,
            $"{buildingLocation.Site.Name} / {buildingLocation.Building.Name}"),
        Location.RoomLocation roomLocation => new(
            roomLocation.Id,
            LocationType.Room,
            LocationPartNotificationModel.FromSite(roomLocation.Site),
            LocationPartNotificationModel.FromBuilding(roomLocation.Building),
            LocationPartNotificationModel.FromRoom(roomLocation.Room),
            $"{roomLocation.Site.Name} / {roomLocation.Building.Name} / {roomLocation.Room.Name}"),
        _ => throw new InvalidOperationException("Unknown location type.")
    };
}

public record LocationPartNotificationModel(Guid Id, string Name, string? Address)
{
    public static LocationPartNotificationModel FromSite(Site site) => new(site.Id, site.Name, site.Address);

    public static LocationPartNotificationModel FromBuilding(Building building) => new(building.Id, building.Name, building.Address);

    public static LocationPartNotificationModel FromRoom(Room room) => new(room.Id, room.Name, null);
}
