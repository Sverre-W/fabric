using Fabric.Hardware.Contracts.Capabilities;
using Fabric.Server.Kiosk.Domain;

namespace Fabric.Server.Kiosk.Application;

public static class KioskDeviceCapabilities
{
    public static string GetRequiredCapability(KioskDeviceType type) => type switch
    {
        KioskDeviceType.QrReader => HardwareCapabilities.QrScan,
        KioskDeviceType.RfidReader => HardwareCapabilities.RfidRead,
        KioskDeviceType.Dispenser => HardwareCapabilities.CardDispense,
        KioskDeviceType.Collector => HardwareCapabilities.CardCollect,
        KioskDeviceType.EidReader => "eid.read",
        KioskDeviceType.PassportReader => HardwareCapabilities.PassportScan,
        KioskDeviceType.LabelPrinter => HardwareCapabilities.LabelPrint,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static string GetCleanupCapability(KioskDeviceType type) => type switch
    {
        KioskDeviceType.Collector => HardwareCapabilities.CardEject,
        _ => GetRequiredCapability(type)
    };
}
