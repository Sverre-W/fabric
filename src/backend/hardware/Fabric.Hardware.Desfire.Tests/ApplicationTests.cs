using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;
using Fabric.Hardware.Desfire.Session;
using Fabric.Hardware.Desfire.Utils;
using Moq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Fabric.Hardware.Desfire.Tests;

public class ApplicationTests
{
    [Fact(DisplayName = "Application settings byte conversion")]
    public void ApplicationSettingsConversionToByte()
    {
        byte actual = (byte)
            new ApplicationSettings
            {
                KeyType = KeyType.Aes,
                ApplicationKeys = 5,
                ExtendedApplicationSettings = true,
                Use2ByteFileIdentifiers = true,
            };

        Assert.Equal(0xB5, actual);
    }

    [Fact(DisplayName = "Extended Application settings byte conversion")]
    public void ExtendedApplicationSettingsConversionToByte()
    {
        byte actual = (byte)
            new ApplicationExtendedSettings
            {
                AdditionalKeySets = true,
                SpecificCapabilityData = false,
                SpecificVcKeys = false,
                CanDeleteApplicationWithApplicationMasterKey = false,
            };

        Assert.Equal(0x01, actual);
    }

    [Fact(DisplayName = "Create conventional application (EV1 Secure Messaging)")]
    public async Task CreateConventionalApplicationEv1()
    {
        //AN12757 pg 107
        ApplicationDescription description = ApplicationDescription
            //            .NewApplication(DesfireApplicationId.Create("1FC1C2"))
            .NewApplication(DesfireApplicationId.Create("F11C2C"))
            .KeySettings(
                new ApplicationKeySettings
                {
                    ChangeKey = ChangeKey.SpecificKey(1),
                    KeySettingsReadOnly = false,
                    MasterKeyReadOnly = false,
                    FreeDirectoryListing = true,
                    AllowCreateAndDeleteWithoutMasterKey = true,
                }
            )
            .Settings(
                new ApplicationSettings
                {
                    KeyType = KeyType.Aes,
                    ApplicationKeys = 5,
                    ExtendedApplicationSettings = true,
                    Use2ByteFileIdentifiers = true,
                }
            )
            .ExtendedSettings(
                new ApplicationExtendedSettings
                {
                    AdditionalKeySets = true,
                    SpecificCapabilityData = false,
                    SpecificVcKeys = false,
                    CanDeleteApplicationWithApplicationMasterKey = false,
                }
            )
            .ActiveKeySetVersion(0)
            .KeySets(4)
            .Only16ByteKeys()
            .KeySetKeySettings(
                new ApplicationKeySettings
                {
                    ChangeKey = ChangeKey.SpecificKey(0),
                    KeySettingsReadOnly = true,
                    MasterKeyReadOnly = true,
                    FreeDirectoryListing = true,
                    AllowCreateAndDeleteWithoutMasterKey = false,
                }
            )
            .IsoFileId(Convert.FromHexString("1122"))
            .IsoDfName(Convert.FromHexString("010203040506"));

        Mock<IRfidEncoder> readerMock = new();
        Mock<ILogger> loggerMock = new();
        DesfireReader reader = new(loggerMock.Object, readerMock.Object);
        reader.Session = new Ev1SecureMessaging(
            loggerMock.Object,
            readerMock.Object,
            DesfireKeyId.ApplicationMasterKey,
            Convert.FromHexString("67BFD95057A206EE8E372511F5E31406"),
            KeyType.Aes
        );

        string expectedCommand = "CA2C1CF11FB501000410021122010203040506";
        byte[] sessionKey = ((Ev1SecureMessaging)reader.Session).SessionKey;
        byte[] iv = new byte[16];

        readerMock
            .Setup(x => x.Send(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[] bytes, CancellationToken token) =>
            {
                byte[] commandCmac = CryptoHelper.Cmac(KeyType.Aes, iv, sessionKey, bytes);
                byte[] responseCmac = CryptoHelper.Cmac(KeyType.Aes, commandCmac, sessionKey, [0x00]);

                return [0x00, ..responseCmac[..8]];
            });

        _ = await reader.CreateApplication(description);

        readerMock.Verify(
            x => x.Send(It.Is<byte[]>(y => Convert.ToHexString(y).Equals(expectedCommand)), It.IsAny<CancellationToken>()),
            Times.Once,
            "Invalid command"
        );
    }
}
