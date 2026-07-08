namespace Fabric.Server.Kiosk.Application;

public sealed class KioskAssetStorageOptions
{
    public string Path { get; set; } = System.IO.Path.Combine("App_Data", "kiosk-assets");
}
