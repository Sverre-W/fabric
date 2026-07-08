namespace Fabric.Server.Kiosk.Domain;

public enum KioskMode
{
    Active,
    Maintenance,
    Disabled
}

public enum KioskSessionStatus
{
    Running,
    Completed,
    Cancelled,
    Failed,
    TimedOut
}

public enum KioskAssetKind
{
    Image,
    Background,
    Logo,
    Video
}

public enum KioskDeviceType
{
    QrReader,
    RfidReader,
    Dispenser,
    Collector,
    EidReader,
    PassportReader,
    LabelPrinter
}
