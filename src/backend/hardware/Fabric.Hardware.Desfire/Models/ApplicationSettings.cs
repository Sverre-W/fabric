using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Models;

public class ApplicationSettings
{
    private int _applicationKeys;

    public ApplicationSettings() { }

    private ApplicationSettings(byte b)
    {
        bool[] bits = BitUtilities.ByteToBits(b);

        KeyType = (bits[7], bits[6]) switch
        {
            (true, false) => KeyType.Aes,
            (false, false) => KeyType.Tdes2K,
            (false, true) => KeyType.Tdes3K,
            (true, true) => throw new NotSupportedException("Invalid key type"),
        };
        ExtendedApplicationSettings = bits[4];
        Use2ByteFileIdentifiers = bits[5];
        bool[] keyRights = new bool[8];
        Array.Copy(bits, 0, keyRights, 0, 4);

        ApplicationKeys = BitUtilities.BitsToByte(keyRights);
    }

    /// <summary>
    ///     If true, files will have a 2 byte ID within the application
    /// </summary>
    public bool Use2ByteFileIdentifiers { get; set; }

    /// <summary>
    ///     If true, extended application settings will be used
    /// </summary>
    public bool ExtendedApplicationSettings { get; set; }

    /// <summary>
    ///     The <see cref="KeyType" /> for ALL keys of the application
    /// </summary>
    public KeyType KeyType { get; set; }

    /// <summary>
    ///     The number of application keys
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public int ApplicationKeys
    {
        get => _applicationKeys;
        set
        {
            if (value is < 0 or > 14)
            {
                throw new ArgumentException("Application can only have 0 to 14 keys");
            }

            _applicationKeys = value;
        }
    }

    private byte AsByte()
    {
        bool[] bits = new bool[8];

        switch (KeyType)
        {
            case KeyType.Aes:
                bits[7] = true;
                bits[6] = false;
                break;

            case KeyType.Tdes2K:
                bits[7] = false;
                bits[6] = false;
                break;

            case KeyType.Tdes3K:
                bits[7] = false;
                bits[6] = true;
                break;

            default:
                throw new ArgumentException($"{KeyType} not supported");
        }

        bits[5] = Use2ByteFileIdentifiers;
        bits[4] = ExtendedApplicationSettings;

        bool[] numberOfKeys = BitUtilities.ByteToBits((byte)ApplicationKeys)[..4];

        Array.Copy(numberOfKeys, 0, bits, 0, 4);

        return BitUtilities.BitsToByte(bits);
    }

    public static explicit operator byte(ApplicationSettings d)
    {
        return d.AsByte();
    }

    public static explicit operator ApplicationSettings(byte b)
    {
        return new ApplicationSettings(b);
    }
}
