namespace Fabric.Server.Infrastructure.Authentication;

public static class ReceptionKioskAuthenticationDefaults
{
    public const string AuthenticationScheme = "ReceptionKiosk";
    public const string Role = "reception-kiosk";
    public const string Policy = "ReceptionKioskOnly";
    public const string KioskIdClaim = "reception-kiosk-id";
    public const string KioskNameClaim = "reception-kiosk-name";
    public const string KioskLocationIdClaim = "reception-kiosk-location-id";
}
