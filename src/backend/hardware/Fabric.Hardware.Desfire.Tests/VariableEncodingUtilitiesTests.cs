using Fabric.Hardware.Desfire.Scripting.Utilities;
using Shouldly;

namespace Fabric.Hardware.Desfire.Tests;

public class VariableEncodingUtilitiesTests
{
    [Fact]
    public void EncodeForFile_should_pack_decimal_text_as_seven_byte_big_endian()
    {
        byte[] encoded = VariableEncodingUtilities.EncodeForFile(System.Text.Encoding.UTF8.GetBytes("47551"), "uint:7:be");

        Convert.ToHexString(encoded).ShouldBe("0000000000B9BF");
    }

    [Fact]
    public void EncodeForFile_should_treat_binary_bytes_as_an_unsigned_integer()
    {
        byte[] encoded = VariableEncodingUtilities.EncodeForFile(Convert.FromHexString("B9BF"), "uint:7:be");

        Convert.ToHexString(encoded).ShouldBe("0000000000B9BF");
    }

    [Fact]
    public void EncodeForFile_should_support_little_endian_padding()
    {
        byte[] encoded = VariableEncodingUtilities.EncodeForFile(System.Text.Encoding.UTF8.GetBytes("47551"), "uint:7:le");

        Convert.ToHexString(encoded).ShouldBe("BFB90000000000");
    }

    [Fact]
    public void EncodeForFile_should_pad_odd_length_hex_with_leading_zero()
    {
        byte[] encoded = VariableEncodingUtilities.EncodeForFile(System.Text.Encoding.UTF8.GetBytes("10001"), "hex");

        Convert.ToHexString(encoded).ShouldBe("010001");
    }
}
