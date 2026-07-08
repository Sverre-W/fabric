using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Models;

public record ApplicationKeySettings
{
    private ApplicationKeySettings(byte b)
    {
        bool[] bits = BitUtilities.ByteToBits(b);

        MasterKeyReadOnly = !bits[0];
        FreeDirectoryListing = bits[1];
        AllowCreateAndDeleteWithoutMasterKey = bits[2];
        KeySettingsReadOnly = !bits[3];

        bool[] keyRights = new bool[8];
        Array.Copy(bits, 4, keyRights, 0, 4);

        ChangeKey = new ChangeKey(BitUtilities.BitsToByte(keyRights));
    }

    public ApplicationKeySettings()
    {
        ChangeKey = ChangeKey.AnyApplicationKey();
    }

    /// <summary>
    ///     The key required to change the key settings
    /// </summary>
    public ChangeKey ChangeKey { get; set; }

    /// <summary>
    ///     If set to true, the settings will be read-only
    /// </summary>
    public bool KeySettingsReadOnly { get; set; }

    /// <summary>
    ///     Allows the creation and deletion of new files without the AMK
    /// </summary>
    public bool AllowCreateAndDeleteWithoutMasterKey { get; set; }

    /// <summary>
    ///     If set to true, the AMK cannot be changed
    /// </summary>
    public bool MasterKeyReadOnly { get; set; }

    /// <summary>
    ///     Indicates if the AMK is required to read file and key settings
    /// </summary>
    public bool FreeDirectoryListing { get; set; }

    private byte ToByte()
    {
        bool[] bits = new bool[8];
        bits[0] = !MasterKeyReadOnly;
        bits[1] = FreeDirectoryListing;
        bits[2] = AllowCreateAndDeleteWithoutMasterKey;
        bits[3] = !KeySettingsReadOnly;

        bool[] changeKeyRights = BitUtilities.ByteToBits(ChangeKey.Key)[..4];
        Array.Copy(changeKeyRights, 0, bits, 4, changeKeyRights.Length);
        return BitUtilities.BitsToByte(bits);
    }

    public static explicit operator byte(ApplicationKeySettings d)
    {
        return d.ToByte();
    }

    public static explicit operator ApplicationKeySettings(byte b)
    {
        return new ApplicationKeySettings(b);
    }
}
