using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Models;

public record PiccKeySettings
{
    private PiccKeySettings(byte b)
    {
        bool[] bits = BitUtilities.ByteToBits(b);

        MasterKeyReadOnly = !bits[0];
        FreeDirectoryListing = bits[1];
        AllowCreateAndDeleteWithoutMasterKey = bits[2];
        KeySettingsReadOnly = !bits[3];
        AllowDamKeys = bits[4];
    }

    public PiccKeySettings() { }

    public bool AllowDamKeys { get; set; } = true;

    /// <summary>
    ///     If set to true, the settings will be read-only
    /// </summary>
    public bool KeySettingsReadOnly { get; set; } = false;

    /// <summary>
    ///     Allows the creation and deletion of new files without the AMK
    /// </summary>
    public bool AllowCreateAndDeleteWithoutMasterKey { get; set; } = true;

    /// <summary>
    ///     If set to true, the AMK cannot be changed
    /// </summary>
    public bool MasterKeyReadOnly { get; set; } = false;

    /// <summary>
    ///     Indicates if the AMK is required to read file and key settings
    /// </summary>
    public bool FreeDirectoryListing { get; set; } = true;

    private byte ToByte()
    {
        bool[] bits = new bool[8];
        bits[0] = !MasterKeyReadOnly;
        bits[1] = FreeDirectoryListing;
        bits[2] = AllowCreateAndDeleteWithoutMasterKey;
        bits[3] = !KeySettingsReadOnly;
        bits[4] = !AllowDamKeys;

        return BitUtilities.BitsToByte(bits);
    }

    public static explicit operator byte(PiccKeySettings d)
    {
        return d.ToByte();
    }

    public static explicit operator PiccKeySettings(byte b)
    {
        return new PiccKeySettings(b);
    }
}
