using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Models;

public record DesfireFileAccessRights
{
    public DesfireFileAccessRights() { }

    public DesfireFileAccessRights(byte[] accessRights)
    {
        if (accessRights.Length != 2)
        {
            throw new ArgumentException();
        }

        bool[] bits = BitUtilities.BytesToBits(accessRights);

        ChangeKey = GetKey(bits, 0);
        ReadWriteKey = GetKey(bits, 4);
        WriteKey = GetKey(bits, 8);
        ReadKey = GetKey(bits, 12);
    }

    public ChangeKey ChangeKey { get; set; } = ChangeKey.AnyApplicationKey();
    public ChangeKey ReadKey { get; set; } = ChangeKey.AnyApplicationKey();
    public ChangeKey WriteKey { get; set; } = ChangeKey.AnyApplicationKey();
    public ChangeKey ReadWriteKey { get; set; } = ChangeKey.AnyApplicationKey();

    private ChangeKey GetKey(bool[] source, int start)
    {
        bool[] keyBuffer = new bool[8];
        Array.Copy(source, start, keyBuffer, 0, 4);
        return (ChangeKey)BitUtilities.BitsToByte(keyBuffer);
    }

    public byte[] GetBytes()
    {
        bool[] bits = new bool[16];
        SetKey(bits, 0, ChangeKey);
        SetKey(bits, 4, ReadWriteKey);
        SetKey(bits, 8, WriteKey);
        SetKey(bits, 12, ReadKey);
        return BitUtilities.BitArrayToByteArray(bits);
    }

    private void SetKey(bool[] bits, int start, ChangeKey key)
    {
        bool[] keyBits = BitUtilities.ByteToBits((byte)key);
        Array.Copy(keyBits, 0, bits, start, 4);
    }
}
