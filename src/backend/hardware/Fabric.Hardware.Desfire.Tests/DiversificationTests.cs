
using Fabric.Hardware.Desfire.Utils;

namespace Fabric.Hardware.Desfire.Tests;

public class DiversificationTests
{
    [Fact(DisplayName = "AES 128 Key Diversification")]
    public void Test()
    {
        //Test the example of AN10922 Page 7
        byte[] masterKey = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");
        byte[] cardId = Convert.FromHexString("04782E21801D80");
        byte[] appId = Convert.FromHexString("3042F5");
        byte[] systemId = Convert.FromHexString("4E585020416275");

        byte[] M = [.. cardId, .. appId, .. systemId];
        byte[] diversified = CryptoHelper.DiversifyAesKey(masterKey, M);

        Assert.Equal("A8DD63A3B89D54B37CA802473FDA9175", Convert.ToHexString(diversified));
    }
}
