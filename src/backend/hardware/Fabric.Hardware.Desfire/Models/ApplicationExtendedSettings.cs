using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Models;

/// <summary>
///     Extended application key settings (KeySett3)
/// </summary>
public class ApplicationExtendedSettings
{
    public ApplicationExtendedSettings() { }

    public ApplicationExtendedSettings(byte b) { }

    /// <summary>
    ///     If true, the AMK can be used to delete this application. Otherwise, it depends on PICC settings
    /// </summary>
    public bool CanDeleteApplicationWithApplicationMasterKey { get; set; }

    /// <summary>
    ///     If true, application specific capability data is enabled
    /// </summary>
    public bool SpecificCapabilityData { get; set; }

    /// <summary>
    ///     If true, application specific VC keys are enabled
    /// </summary>
    public bool SpecificVcKeys { get; set; }

    /// <summary>
    ///     Indicates there is more than one key set for this application
    /// </summary>
    public bool AdditionalKeySets { get; set; }

    private byte ToByte()
    {
        bool[] bits = new bool[8];
        bits[4] = CanDeleteApplicationWithApplicationMasterKey;
        bits[2] = SpecificCapabilityData;
        bits[1] = SpecificVcKeys;
        bits[0] = AdditionalKeySets;

        return BitUtilities.BitsToByte(bits);
    }

    public static explicit operator byte(ApplicationExtendedSettings d)
    {
        return d.ToByte();
    }

    public static explicit operator ApplicationExtendedSettings(byte b)
    {
        return new ApplicationExtendedSettings(b);
    }
}
