using Microsoft.Extensions.Logging;
using Moq;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;
using Fabric.Hardware.Desfire.Session;

namespace Fabric.Hardware.Desfire.Tests;

public class DesfireFileTests
{
    [Theory(DisplayName = "Desfire file options conversion")]
    [InlineData(CommunicationMode.Plain, true, true)]
    [InlineData(CommunicationMode.Plain, false, true)]
    [InlineData(CommunicationMode.Plain, true, false)]
    [InlineData(CommunicationMode.Plain, false, false)]
    [InlineData(CommunicationMode.Cmac, true, true)]
    [InlineData(CommunicationMode.Cmac, false, true)]
    [InlineData(CommunicationMode.Cmac, true, false)]
    [InlineData(CommunicationMode.Cmac, false, false)]
    [InlineData(CommunicationMode.Enciphered, true, true)]
    [InlineData(CommunicationMode.Enciphered, false, true)]
    [InlineData(CommunicationMode.Enciphered, true, false)]
    [InlineData(CommunicationMode.Enciphered, false, false)]
    public void DesfireOptionsIdentityPreserved(CommunicationMode mode, bool accessRights, bool dynamicMessaging)
    {
        DesfireFileOptions fileOptions = new()
        {
            CommunicationMode = mode,
            AdditionalAccessRights = accessRights,
            SecureDynamicMessaging = dynamicMessaging,
        };

        DesfireFileOptions fileOptions2 = new((byte)fileOptions);

        Assert.Equal(fileOptions, fileOptions2);
    }

    [Fact(DisplayName = "Desfire file access rights conversion")]
    public void DesfireAccessRightsIdentityPreserved()
    {
        DesfireFileAccessRights expected = new()
        {
            ChangeKey = ChangeKey.AnyApplicationKey(),
            ReadWriteKey = ChangeKey.ReadOnly(),
            WriteKey = ChangeKey.SpecificKey(0x05),
            ReadKey = ChangeKey.SpecificKey(0x04),
        };

        byte[] bits = expected.GetBytes();
        DesfireFileAccessRights actual = new(bits);

        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "Write cmac file (D40)")]
    public async Task D40WriteDataToFile()
    {
        byte[] data = Convert.FromHexString("00112233445566778899AABBCCDD");

        //AN588114-AN12757: Table 32
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new D40SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                KeyType.Tdes2K,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("A908E976D518D0B9AE1D7F7A04CB0154")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("3D010000000E000000112233445566778899AABBCCDD87480163");

        _ = readerMock.Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(Convert.FromHexString("00"));

        _ = await reader.WriteData(1, data, CommunicationMode.Cmac);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Write plain file in ISO-safe chunks")]
    public async Task PlainWriteDataIsChunkedForIsoApduLimits()
    {
        byte[] data = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();

        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);

        List<byte[]> sent = [];
        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => sent.Add([.. bytes]))
            .ReturnsAsync([0x00]);

        _ = await reader.WriteData(1, data, CommunicationMode.Plain);

        Assert.Equal(2, sent.Count);
        Assert.Equal(244, sent[0].Length);
        Assert.Equal(28, sent[1].Length);
        Assert.Equal((byte)DesfireCommand.WriteData, sent[0][0]);
        Assert.Equal((byte)DesfireCommand.WriteData, sent[1][0]);
        Assert.Equal((byte)0x01, sent[0][1]);
        Assert.Equal((byte)0x01, sent[1][1]);
        Assert.Equal((byte)0xEC, sent[0][5]);
        Assert.Equal((byte)0x14, sent[1][5]);
    }

    [Fact(DisplayName = "Write enciphered file in ISO-safe chunks")]
    public async Task EncipheredWriteDataIsChunkedForIsoApduLimits()
    {
        byte[] data = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();

        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("00000000000000000000000000000000"),
                KeyType.Aes
            ),
        };

        List<byte[]> sent = [];
        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => sent.Add([.. bytes]))
            .ReturnsAsync([0x00]);

        _ = await reader.WriteData(1, data, CommunicationMode.Enciphered);

        Assert.Equal(2, sent.Count);
        Assert.Equal(152, sent[0].Length);
        Assert.Equal(152, sent[1].Length);
        Assert.Equal((byte)DesfireCommand.WriteData, sent[0][0]);
        Assert.Equal((byte)DesfireCommand.WriteData, sent[1][0]);
        Assert.Equal((byte)0x01, sent[0][1]);
        Assert.Equal((byte)0x01, sent[1][1]);
        Assert.Equal((byte)0x80, sent[0][5]);
        Assert.Equal((byte)0x80, sent[1][5]);
    }

    [Fact(DisplayName = "Write operation uses actual variable length")]
    public async Task WriteToFileOperationUsesActualVariableLength()
    {
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("00000000000000000000000000000000"),
                KeyType.Aes
            ),
        };

        List<byte[]> sent = [];
        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => sent.Add([.. bytes]))
            .ReturnsAsync([0x00]);

        ExecutionState state = new()
        {
            Variables =
            {
                ["badgeNumber"] = System.Text.Encoding.UTF8.GetBytes("ABCDEF"),
            },
        };

        WriteToFileOperation operation = new(1, CommunicationMode.Enciphered, "badgeNumber", "text", 0, 256);

        _ = await operation.Execute(state, reader);

        Assert.Single(sent);
        Assert.Equal((byte)0x06, sent[0][5]);
        Assert.Equal(24, sent[0].Length);
    }

    [Fact(DisplayName = "Read cmac file (D40)")]
    public async Task D40ReadDataOfCmacFile()
    {
        byte[] expectedData = Convert.FromHexString("00112233445566778899AABBCCDD");

        //AN588114-AN12757: Table 33
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new D40SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                KeyType.Tdes2K,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("A908E976D518D0B9AE1D7F7A04CB0154")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("BD010000000E0000");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("0000112233445566778899AABBCCDD87480163"));

        IDesfireResponse<byte[]> data = await reader.ReadData(1, 0, expectedData.Length, CommunicationMode.Cmac);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        Assert.Equal(DesfireStatusCode.Success, data.StatusCode);
        Assert.Equal(expectedData, data.Data);
    }

    [Fact(DisplayName = "Write encrypted file (D40)", Skip = "Have to implement Native Desfire Enchipering")]
    public async Task D40WriteEncryptedFile()
    {
        byte[] data = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");

        //AN588114-AN12757: Table 34
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new D40SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                KeyType.Tdes2K,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("A908E976D518D0B9AE1D7F7A04CB0154")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("3D01000000100000C9F94412DA6FEBA4B97995E9DF7AFB9A577CE04B002BFB65");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, token) => Console.WriteLine(bytes))
            .ReturnsAsync(Convert.FromHexString("00"));

        _ = await reader.WriteData(1, data, CommunicationMode.Enciphered);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Write cmac file (EV1) (2TDEA)")]
    public async Task Ev12TdeaWriteCmacFile()
    {
        byte[] data = Convert.FromHexString("010203040506070809101112131415161718");

        //AN588114-AN12757: Table 21
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("7EBBEA1BA4A399EF7EBBEA1BA4A399EF"),
                KeyType.Tdes2K
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("3D01000000120000010203040506070809101112131415161718C15BFD83F9021869");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], CancellationToken>((bytes, _) => Console.WriteLine(bytes))
            .ReturnsAsync(Convert.FromHexString("00AD37516C1029640E"));

        _ = await reader.WriteData(1, data, CommunicationMode.Cmac);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Read cmac file (EV1) (2TDEA)")]
    public async Task Ev12TdeaReadCmacFile()
    {
        byte[] data = Convert.FromHexString("010203040506070809101112131415161718");

        //AN588114-AN12757: Table 22
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("7EBBEA1BA4A399EF7EBBEA1BA4A399EF"),
                KeyType.Tdes2K,
                Convert.FromHexString("AD37516C1029640E")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("BD01000000120000");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("0001020304050607080910111213141516171835E4EE4183A3B718"));

        IDesfireResponse<byte[]> response = await reader.ReadData(1, 0, data.Length, CommunicationMode.Cmac);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        Assert.Equal(data, response.Data);
    }

    [Fact(DisplayName = "Write cmac file (EV1) (AES)")]
    public async Task Ev1AesWriteCmacFile()
    {
        byte[] data = Convert.FromHexString("0102030405060708090A0B0C0D0E0F101112131415");

        //AN588114-AN12757: Table 23
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("0AAF803DD2A6252D851B69B9E4F63801"),
                KeyType.Aes,
                Convert.FromHexString("00000000000000000000000000000000")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("3D030000001500000102030405060708090A0B0C0D0E0F101112131415BEF5B687604234F7");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("00A6589DC8FC47D2E4"));

        _ = await reader.WriteData(3, data, CommunicationMode.Cmac);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Read cmac file (EV1) (AES)")]
    public async Task Ev1AesReadCmacFile()
    {
        byte[] data = Convert.FromHexString("0102030405060708090A0B0C0D0E0F101112131415");

        //AN588114-AN12757: Table 24
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("0AAF803DD2A6252D851B69B9E4F63801"),
                KeyType.Aes,
                Convert.FromHexString("A6589DC8FC47D2E4F37AF645C3EBA07D")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("BD03000000150000");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("000102030405060708090A0B0C0D0E0F101112131415DF5C0EA3D6331E4F"));

        IDesfireResponse<byte[]> response = await reader.ReadData(3, 0, data.Length, CommunicationMode.Cmac);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        Assert.Equal(data, response.Data);
    }

    [Fact(DisplayName = "Write encrypted file (EV1) (2TDEA)")]
    public async Task Ev12TdeaWriteEncryptedFile()
    {
        byte[] data = Convert.FromHexString("010203040506070809101112131415161718");

        //AN588114-AN12757: Table 25
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("B736960AFA41EED7B736960AFA41EED7"),
                KeyType.Tdes2K,
                Convert.FromHexString("0000000000000000")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("3D00000000120000EF4E0879067770A638F8D5DFFBCC7A5D95A66B761BCBEAE3");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("00B206E23D3083EF51"));

        _ = await reader.WriteData(0, data, CommunicationMode.Enciphered);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Read encrypted file (EV1) (2TDEA)")]
    public async Task Ev12TdeaReadEncryptedFile()
    {
        byte[] data = Convert.FromHexString("010203040506070809101112131415161718");

        //AN588114-AN12757: Table 26
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("B736960AFA41EED7B736960AFA41EED7"),
                KeyType.Tdes2K,
                Convert.FromHexString("B206E23D3083EF51")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("BD00000000120000");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("006423F66793402B1D3B6AAD6CA3EE028D79C11B8ADF793C1C"));

        IDesfireResponse<byte[]> response = await reader.ReadData(0, 0, data.Length, CommunicationMode.Enciphered);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        Assert.Equal(data, response.Data);
    }

    [Fact(DisplayName = "Write encrypted file (EV1) (AES)")]
    public async Task Ev12AesWriteEncryptedFile()
    {
        byte[] data = Convert.FromHexString("112233445566778899AABBCCDDEEFF");

        //AN588114-AN12757: Table 27
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("227EAC508FB2D130E0451988D51F8DD0"),
                KeyType.Aes,
                Convert.FromHexString("00000000000000000000000000000000")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("3D020000000F00009B9915A7572364D05CDF03FBA9B0F69EA88E3C4F0DEE3EDAB9ACC8BE88B7DAC9");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("00B987074D3F60EEF6"));

        _ = await reader.WriteData(2, data, CommunicationMode.Enciphered);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }

    [Fact(DisplayName = "Read encrypted file (EV1) (AES)")]
    public async Task Ev1AesReadEncryptedFile()
    {
        byte[] data = Convert.FromHexString("112233445566778899AABBCCDDEEFF");

        //AN588114-AN12757: Table 28
        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object)
        {
            Session = new Ev1SecureMessaging(
                loggerMock.Object,
                readerMock.Object,
                DesfireKeyId.ApplicationMasterKey,
                Convert.FromHexString("227EAC508FB2D130E0451988D51F8DD0"),
                KeyType.Aes,
                Convert.FromHexString("B987074D3F60EEF6C18034551AA79E09")
            ),
        };

        byte[] expectedCommand = Convert.FromHexString("BD020000000F0000");

        _ = readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Convert.FromHexString("003C624B56FF16798F960E341E10EEA8CF5155D67AC6273EE5DDEB90E16228DF03"));

        IDesfireResponse<byte[]> response = await reader.ReadData(2, 0, data.Length, CommunicationMode.Enciphered);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => y.SequenceEqual(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );

        Assert.Equal(data, response.Data);
    }
}
