using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Models;

public struct DesfireFileOptions
{
    public DesfireFileOptions() { }

    public DesfireFileOptions(byte b)
    {
        bool[] bits = BitUtilities.ByteToBits(b);

        CommunicationMode = (bits[0], bits[1]) switch
        {
            (false, false) => CommunicationMode.Plain,
            (true, false) => CommunicationMode.Cmac,
            (true, true) => CommunicationMode.Enciphered,
            _ => throw new NotSupportedException("Unsupported communication mode."),
        };

        AdditionalAccessRights = bits[7];
        SecureDynamicMessaging = bits[6];
    }

    public CommunicationMode CommunicationMode { get; set; }
    public bool AdditionalAccessRights { get; set; }
    public bool SecureDynamicMessaging { get; set; }

    private byte ToByte()
    {
        bool[] data = new bool[8];

        switch (CommunicationMode)
        {
            case CommunicationMode.Plain:
                data[0] = false;
                data[1] = false;
                break;
            case CommunicationMode.Cmac:
                data[0] = true;
                data[1] = false;
                break;
            case CommunicationMode.Enciphered:
                data[0] = true;
                data[1] = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        data[7] = AdditionalAccessRights;
        data[6] = SecureDynamicMessaging;

        //data 2..5 is RFU

        return BitUtilities.BitsToByte(data);
    }

    public static explicit operator byte(DesfireFileOptions d)
    {
        return d.ToByte();
    }

    public static explicit operator DesfireFileOptions(byte b)
    {
        return new DesfireFileOptions(b);
    }
}
