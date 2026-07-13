namespace Fabric.Server.Kiosk.Domain;

public enum KioskMode
{
    Active,
    Maintenance,
    Disabled
}

public enum KioskSessionStatus
{
    Starting,
    Running,
    Completed,
    Cancelled,
    Failed,
    TimedOut
}

public enum KioskSessionCancellationSource
{
    UserHome,
    Guard,
    Maintenance,
    Disabled,
    WorkflowCancelled
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
    Encoder,
    EidReader,
    PassportReader,
    LabelPrinter
}
