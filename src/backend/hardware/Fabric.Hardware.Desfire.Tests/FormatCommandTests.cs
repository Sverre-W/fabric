
/*
namespace Fabric.Hardware.Desfire.Tests;

public class FormatCommandTests
{
    [Fact]
    public void ResolvePiccKey_normalizes_hex_prefixes_and_spacing()
    {
        string key = Format.ResolvePiccKey(" hex: 00 11 22 33 44 55 66 77 88 99 aa bb cc dd ee ff ", Fabric.Hardware.Desfire.Protocol.KeyType.Aes);

        key.ShouldBe("00112233445566778899AABBCCDDEEFF");
    }

    [Fact]
    public void ResolvePiccKey_validates_length_for_tdes2k()
    {
        Should.Throw<ArgumentException>(() => Format.ResolvePiccKey("0011223344556677", Fabric.Hardware.Desfire.Protocol.KeyType.Tdes2K));
    }

    [Fact]
    public void ResolveKeyType_defaults_to_aes()
    {
        Format.ResolveKeyType(false, false).ShouldBe(Fabric.Hardware.Desfire.Protocol.KeyType.Aes);
    }

    [Fact]
    public void ResolveKeyType_prefers_tdes2k_when_requested()
    {
        Format.ResolveKeyType(false, true).ShouldBe(Fabric.Hardware.Desfire.Protocol.KeyType.Tdes2K);
    }
}
*/
