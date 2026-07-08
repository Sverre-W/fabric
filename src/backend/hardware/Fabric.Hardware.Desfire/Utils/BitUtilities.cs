namespace Fabric.Hardware.Desfire.Utils;

public static class BitUtilities
{
    public static byte[] BitArrayToByteArray(bool[] bits)
    {
        byte[] bytes = new byte[(bits.Length + 7) / 8];

        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                bytes[i / 8] |= (byte)(1 << (i % 8));
            }
        }

        return bytes;
    }

    public static byte BitsToByte(bool[] bits)
    {
        if (bits.Length > 8)
        {
            throw new ArgumentException("Cannot convert to a single bit.");
        }

        byte value = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                value |= (byte)(1 << i);
            }
        }

        return value;
    }

    public static bool[] ByteToBits(byte b)
    {
        bool[] bits = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            bits[i] = (b & (1 << i)) != 0;
        }

        return bits;
    }

    public static bool[] BytesToBits(byte[] bytes)
    {
        bool[] bits = new bool[bytes.Length * 8];

        for (int i = 0; i < bytes.Length; i++)
        {
            bool[] byteBits = ByteToBits(bytes[i]);
            Array.Copy(byteBits, 0, bits, i * 8, 8);
        }

        return bits;
    }

    /// <summary>
    ///     Converts to integer value to a bit and ensure bits larger the bit length are 0
    /// </summary>
    /// <param name="value"></param>
    /// <param name="bitLength"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static byte ByteFromInt(int value, int bitLength)
    {
        if (bitLength is > 8 or < 1)
        {
            throw new ArgumentException("Cannot convert to a single bit.");
        }

        int mask = (1 << bitLength) - 1;
        return (byte)(value & mask);
    }

    public static string BitsToString(bool[] bits)
    {
        char[] chars = new char[bits.Length];

        for (int i = 0; i < bits.Length; i++)
        {
            chars[i] = bits[i] ? '1' : '0';
        }

        return new string(chars);
    }

    public static byte[] XorByteArray(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException("Byte arrays must be of the same length");
        }

        byte[] result = new byte[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = (byte)(a[i] ^ b[i]);
        }

        return result;
    }
}
