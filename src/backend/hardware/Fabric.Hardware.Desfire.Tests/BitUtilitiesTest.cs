using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Tests;

public class BitUtilitiesTest
{
    [Fact(DisplayName = "Converting a bytes leads to LSB first")]
    public void TestByteToBitArrayLeastSignificantFirst()
    {
        byte value = 0x02;

        bool[] bits = BitUtilities.ByteToBits(value);
        bool[] expected = [false, true, false, false, false, false, false, false];

        Assert.Equal(expected, bits);
    }

    [Fact(DisplayName = "Converting bytes results in LSB first")]
    public void TestBytesToBitsLeastSignificantFirst()
    {
        byte[] intBytes = BitConverter.GetBytes(514)[..2];

        bool[] bits = BitUtilities.BytesToBits(intBytes);
        bool[] expected = [false, true, false, false, false, false, false, false, false, true, false, false, false, false, false, false];

        Assert.Equal(expected, bits);
    }

    [Theory(DisplayName = "Bits to bytes conversion")]
    [InlineData(0xF)]
    [InlineData(0xA)]
    [InlineData(0x7)]
    [InlineData(0x3)]
    public void PreserveIdentityAfterConversion(byte expected)
    {
        bool[] bits = BitUtilities.ByteToBits(expected);
        byte actual = BitUtilities.BitsToByte(bits);

        Assert.Equal(expected, actual);
    }

    /// <summary>
    ///     These values are taken from various examples in AN588114-AN12757
    /// </summary>
    [Theory(DisplayName = "XOR keys")]
    [InlineData("00112233445566778899AABBCCDDEEFF", "A0A1A2A3A4A5A6A7A8A9AAABACADAEAF", "A0B08090E0F0C0D02030001060704050")]
    public void TestXorKeys(string key1, string key2, string expected)
    {
        byte[] actual = BitUtilities.XorByteArray(Convert.FromHexString(key1), Convert.FromHexString(key2));

        byte[] expectedBytes = Convert.FromHexString(expected);
        Assert.Equal(expectedBytes, actual);
    }
}
