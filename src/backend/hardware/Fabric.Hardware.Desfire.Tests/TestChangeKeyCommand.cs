using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;
using Fabric.Hardware.Desfire.Session;
using Microsoft.Extensions.Logging;
using Moq;


namespace Fabric.Hardware.Desfire.Tests;

public class TestChangeKeyCommand
{
    private readonly DesfireReader _reader;
    private readonly Mock<IRfidEncoder> _readerMock;
    private readonly Mock<ILogger> _loggerMock;

    public TestChangeKeyCommand()
    {
        _readerMock = new Mock<IRfidEncoder>();
        _loggerMock = new();
        _reader = new DesfireReader(_loggerMock.Object, _readerMock.Object);
    }

    [Fact(DisplayName = "Change TDES PICC Key to AES")]
    public async Task TestChangePiccKeyTdesToAes()
    {
        _readerMock.Reset();

        //AN588114-AN12757: Table 93
        byte[] sessionKey = Convert.FromHexString("DAFF378C024610FBDAFF378C024610FB");
        byte[] iv = Convert.FromHexString("0000000000000000");

        _reader.Session = (Ev1SecureMessaging)new(_loggerMock.Object, _readerMock.Object, 0x00, sessionKey, KeyType.Tdes2K, iv);

        byte[] newKey = Convert.FromHexString("00000000000000000000000000000000");
        byte[] expectedCommand = Convert.FromHexString("C480440FE29FF2E51AD18AA105C125CDC1F3B81590984219EAB1");

        _readerMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(Convert.FromHexString("00"));

        await _reader.ChangePiccKey(KeyType.Aes, newKey, 0x00, CancellationToken.None);

        _readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Change AES Key EV1 (Target Key != Current Key)")]
    public async Task TestChangeAesKeyCommandDifferentKeysEv1()
    {
        _readerMock.Reset();

        //AN588114-AN12757: Table 95
        byte[] sessionKey = Convert.FromHexString("5DD4CBFC20A1988E1CDBAE322B315D57");
        byte[] iv = Convert.FromHexString("70873F2D468019A38E7184FE5208F607");

        _reader.Session = (Ev1SecureMessaging)new(_loggerMock.Object, _readerMock.Object, 0x00, sessionKey, KeyType.Aes, iv);

        byte[] oldKey = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");
        byte[] newKey = Convert.FromHexString("A0A1A2A3A4A5A6A7A8A9AAABACADAEAF");
        byte[] expectedCommand = Convert.FromHexString("C401E4CDFCD2D335CCAAB4BB602805B9DAAFFB1B84FE4E4952DD0B9E2F20BB4C789C");

        _readerMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(Convert.FromHexString("0002BC7F4ACA83DA63"));

        await _reader.ChangeKey(KeyType.Aes, DesfireKeyId.Number(1), oldKey, newKey, 0x03);

        _readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        _reader.Session.GetType().ShouldBe(typeof(Ev1SecureMessaging), "the session should remain active");
    }

    [Fact(DisplayName = "Change AES Key EV1 (Target Key = Current Key)")]
    public async Task TestChangeAesKeyCommandSameKeyEv1_2()
    {
        _readerMock.Reset();

        //AN588114-AN12757: Table 96
        _reader.Session = (Ev1SecureMessaging)
            new(_loggerMock.Object, _readerMock.Object, 0x00, Convert.FromHexString("04BC99A81DB7293FAA86CA225ECD7660"), KeyType.Aes);

        byte[] oldKey = Convert.FromHexString("A0A1A2A3A4A5A6A7A8A9AAABACADAEAF");
        byte[] newKey = Convert.FromHexString("B0B1B2B3B4B5B6B7B8B9BABBBCBDBEBF");
        byte[] expectedCommand = Convert.FromHexString("C400B679B08051DA0A62D14C3544A968C943636A2815165E1FB7C1DCC839CFCD3D94");

        _readerMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        await _reader.ChangeKey(KeyType.Aes, DesfireKeyId.ApplicationMasterKey, oldKey, newKey, 0x1);

        _readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        _reader.Session.GetType().ShouldBe(typeof(PlainDesfireSession), "the session is lost after changing current key");
    }

    [Fact(DisplayName = "Change AES Key EV1 (Target Key = Current Key)")]
    public async Task TestChangeAesKeyCommandSameKeyEv1()
    {
        _readerMock.Reset();

        //AN0945 pg 76
        _reader.Session = (Ev1SecureMessaging)
            new(_loggerMock.Object, _readerMock.Object, 0x00, Convert.FromHexString("04BC99A81DB7293FAA86CA225ECD7660"), KeyType.Aes);

        byte[] oldKey = Convert.FromHexString("A0A1A2A3A4A5A6A7A8A9AAABACADAEAF");
        byte[] newKey = Convert.FromHexString("B0B1B2B3B4B5B6B7B8B9BABBBCBDBEBF");
        byte[] expectedCommand = Convert.FromHexString("C400B679B08051DA0A62D14C3544A968C943636A2815165E1FB7C1DCC839CFCD3D94");

        _readerMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync([0x00]);

        await _reader.ChangeKey(KeyType.Aes, DesfireKeyId.ApplicationMasterKey, oldKey, newKey, 0x1);

        _readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        _reader.Session.GetType().ShouldBe(typeof(PlainDesfireSession), "the session is lost after changing current key");
    }
}
