namespace Fabric.Server.Infrastructure.Authentication;

public static class KioskAuthenticationDefaults
{
    public const string AuthenticationScheme = "Kiosk";
    public const string Role = "kiosk";
    public const string Policy = "KioskOnly";
    public const string KioskIdClaim = "kiosk-id";
    public const string KioskNameClaim = "kiosk-name";
    public const string KioskProfileIdClaim = "kiosk-profile-id";
}
