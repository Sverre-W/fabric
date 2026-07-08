using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;
using Fabric.Hardware.Desfire.Session;
using Microsoft.Extensions.Logging;
using Moq;


namespace Fabric.Hardware.Desfire.Tests;

public class DesfireAuthenticationTests
{
    [Fact(DisplayName = "Authenticate Iso (EV1) Default 2TDEA Key")]
    public async Task Ev1SecureMessaging2KDesAuthentication()
    {
        //AN588114-AN12757: Table 20
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);

        const string expectedSessionKey = "FE07A026682EB9D7FE07A026682EB9D7";

        readerMock
            .SetupSequence(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("AFD3C84E1AAB3AA692"))
            .ReturnsAsync(Convert.FromHexString("00DC9A5F5B3F8DE04E"));

        IDesfireResponse response = await reader.AuthenticateIso(
            DesfireKeyId.Number(0x00),
            Convert.FromHexString("00000000000000000000000000000000"),
            KeyType.Tdes2K,
            Convert.FromHexString("FE07A026547F0BAE")
        );

        response.IsSuccess.ShouldBe(true, "the authentication should succeed");
        reader.Session.GetType().ShouldBe(typeof(Ev1SecureMessaging), "the session should be authenticated");

        string actual = Convert.ToHexString((reader.Session as Ev1SecureMessaging)!.SessionKey);

        actual.ShouldBe(expectedSessionKey);
    }

    [Fact(DisplayName = "Authenticate AES (EV1) Default AES Key")]
    public async Task Ev1SecureMessagingAesAuthentication()
    {
        //AN588114-AN12757: Table 20
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);

        const string expectedSessionKey = "2347C1551EA0353A9D965CA7B52FCA84";

        readerMock
            .SetupSequence(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("AFC5537C8EFFFCC7E152C27831AFD383BA"))
            .ReturnsAsync(Convert.FromHexString("00FFC212245F03DB0EA0645A495190952A"));

        IDesfireResponse response = await reader.AuthenticateAes(
            DesfireKeyId.Number(0x02),
            Convert.FromHexString("00000000000000000000000000000000"),
            Convert.FromHexString("2347C1557F80707ABDFF86BF9D965CA7")
        );

        response.IsSuccess.ShouldBe(true, "the authentication should succeed");
        reader.Session.GetType().ShouldBe(typeof(Ev1SecureMessaging), "the session should be authenticated");

        string actual = Convert.ToHexString((reader.Session as Ev1SecureMessaging)!.SessionKey);

        actual.ShouldBe(expectedSessionKey);
    }

    [Fact(DisplayName = "Authenticate (D40) Default 2TDEA Key")]
    public async Task D40SecureMessaging2TdeaAuthentication()
    {
        //AN588114-AN12757: Table 30
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);

        byte[] expectedSessionKey = Convert.FromHexString("CB4B2F5AFC68213ECB4B2F5AFC68213E");

        readerMock
            .SetupSequence(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("AF07B4246843F94E2F"))
            .ReturnsAsync(Convert.FromHexString("000F3E03224EE6260F"));

        IDesfireResponse response = await reader.Authenticate(
            DesfireKeyId.ApplicationMasterKey,
            Convert.FromHexString("00000000000000000000000000000000"),
            KeyType.Tdes2K,
            Convert.FromHexString("CB4B2F5A73160F44")
        );

        response.IsSuccess.ShouldBe(true, "the authentication should succeed");
        reader.Session.GetType().ShouldBe(typeof(D40SecureMessaging), "the session should be authenticated");

        byte[]? actual = (reader.Session as D40SecureMessaging)?.SessionKey;

        actual.ShouldBe(expectedSessionKey);
    }

    [Fact(DisplayName = "Authenticate (D40) Non Default 2TDEA Key")]
    public async Task D40SecureMessaging2TdeaAuthenticationNonDefaultKey()
    {
        //AN588114-AN12757: Table 31
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);

        const string expectedSessionKey = "2AEC97F262C615A024313D2485C043FD";

        readerMock
            .SetupSequence(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("AF43F0384069C1CBB2"))
            .ReturnsAsync(Convert.FromHexString("00179F286709BB449E"));

        IDesfireResponse response = await reader.Authenticate(
            DesfireKeyId.Number(0x02),
            Convert.FromHexString("00112233445566778899AABBCCDDEEFF"),
            KeyType.Tdes2K,
            Convert.FromHexString("2AEC97F224313D24")
        );

        response.IsSuccess.ShouldBe(true, "the authentication should succeed");
        reader.Session.GetType().ShouldBe(typeof(D40SecureMessaging), "the session should be authenticated");

        string actual = Convert.ToHexString((reader.Session as D40SecureMessaging)!.SessionKey);

        actual.ShouldBe(expectedSessionKey);
    }

    [Fact(DisplayName = "Authenticate Ev2 First Default AES Key", Skip = "Implement Ev2")]
    public async Task Ev2SecureMessagingAesFirstAuthenticationNonDefaultKey()
    {
        //AN588114-AN12757: Table 12
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);

        const string expectedSessionKey = "030A8C76BBC954513A52B2AAD161D15B";
        const string expectedMacingKey = "185D0E3CEA4C0C32DFAD84B3414A5054";
        const string expectedTransactionIdentifier = "B04D6C11";
        const string expectedRequestPart1 = "710000";
        const string expectedRequestPart2 = "AF60DCFF9A0BBE8DE18B7D79BB590CD4B42D531C24906D1B0D11BEB17E8850D298";

        readerMock
            .SetupSequence(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("AFC56F576D2444171CF64B196346A81662"))
            .ReturnsAsync(Convert.FromHexString("00EE93375DE2190A24F97D4AE363CAEC8DE2ED76DF4C3EE23C9D3499E3EC8D2259"));

        IDesfireResponse response = await reader.AuthenticateEv2(
            DesfireKeyId.ApplicationMasterKey,
            Convert.FromHexString("00000000000000000000000000000000"),
            Convert.FromHexString("876D85B7FC717073AFBF564834F98F1E")
        );

        response.IsSuccess.ShouldBe(true, "the authentication should succeed");
        reader.Session.GetType().ShouldBe(typeof(Ev2SecureMessaging), "the session should be authenticated");

        readerMock.Verify(
            x =>
                x.Send(
                    It.Is<byte[]>(y => Convert.ToHexString(y).Equals(expectedRequestPart1, StringComparison.Ordinal)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once,
            "Invalid command part 1"
        );

        readerMock.Verify(
            x =>
                x.Send(
                    It.Is<byte[]>(y => Convert.ToHexString(y).Equals(expectedRequestPart2, StringComparison.Ordinal)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once,
            "Invalid command part 2"
        );

        Ev2SecureMessaging session = (reader.Session as Ev2SecureMessaging)!;
        string actualSessionKey = Convert.ToHexString(session.SessionKey);
        string actualMacingKey = Convert.ToHexString(session.MacingKey);
        string actualTransactionId = Convert.ToHexString(session.TransactionId);
        ushort actualCommandCount = session.CommandCounter;

        actualSessionKey.ShouldBeEquivalentTo(expectedSessionKey);
        actualMacingKey.ShouldBeEquivalentTo(expectedMacingKey);
        actualTransactionId.ShouldBeEquivalentTo(expectedTransactionIdentifier);
        actualCommandCount.ShouldBe((ushort)0);
    }
}
