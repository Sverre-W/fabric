using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Utils;


namespace Fabric.Hardware.Desfire.Tests;

public class CryptoHelperTests
{
    [Theory(DisplayName = "Valid CRC16")]
    [InlineData("00112233445566778899AABBCCDDEEFF", "CC69")]
    public void Crc16Valid(string dataStr, string crc)
    {
        byte[] data = Convert.FromHexString(dataStr);
        byte[] crc16bytes = CryptoHelper.CalculateCrc16(data);
        string actualCrc = Convert.ToHexString(crc16bytes).ToUpperInvariant();

        Assert.Equal(crc, actualCrc);

        data.ShouldBeEquivalentTo(Convert.FromHexString(dataStr), "Crc16 calculation should not change input data");
    }

    [Theory(DisplayName = "Valid CMAC")]
    [InlineData(
        KeyType.Tdes2K,
        "0000000000000000",
        "7EBBEA1BA4A399EF7EBBEA1BA4A399EF",
        "3D01000000120000010203040506070809101112131415161718",
        "C15BFD83F9021869"
    )]
    [InlineData(KeyType.Tdes2K, "AD37516C1029640E", "7EBBEA1BA4A399EF7EBBEA1BA4A399EF", "BD01000000120000", "B66E1C8EEA07C950")]
    [InlineData(
        KeyType.Aes,
        "CB9E26FB8175742BA56843906C883F50",
        "0AAF803DD2A6252D851B69B9E4F63801",
        "0102030405060708090A0B0C0D0E0F10111213141500",
        "DF5C0EA3D6331E4F"
    )]
    public void CmacValid(KeyType keyType, string ivStr, string sessionKey, string dataStr, string cmac)
    {
        byte[] data = Convert.FromHexString(dataStr);
        byte[] iv = Convert.FromHexString(ivStr);
        byte[] cmacBytes = CryptoHelper.Cmac(keyType, iv, Convert.FromHexString(sessionKey), data);
        string actualCmac = Convert.ToHexString(cmacBytes[..8]).ToUpperInvariant();

        Assert.Equal(cmac, actualCmac);

        iv.ShouldBeEquivalentTo(Convert.FromHexString(ivStr), "CMAC calculation should not change input data");

        data.ShouldBeEquivalentTo(Convert.FromHexString(dataStr), "CMAC calculation should not change input data");
    }

    [Theory(DisplayName = "Valid CRC32")]
    [InlineData("3D00000000120000010203040506070809101112131415161718", "9C1AF759")]
    [InlineData("112233445566778899AABBCCDDEEFF00", "23E7F06F")]
    public void Crc32Valid(string dataStr, string crc)
    {
        byte[] data = Convert.FromHexString(dataStr);
        byte[] crc32bytes = CryptoHelper.CalculateCrc32(data);
        string actualCrc = Convert.ToHexString(crc32bytes).ToUpperInvariant();

        Assert.Equal(crc, actualCrc);

        data.ShouldBeEquivalentTo(Convert.FromHexString(dataStr), "Crc32 calculation should not change input data");
    }
}
