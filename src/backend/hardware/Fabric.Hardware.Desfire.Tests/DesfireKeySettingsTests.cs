using Fabric.Hardware.Desfire.Models;

namespace Fabric.Hardware.Desfire.Tests;

public class DesfireKeySettingsTests
{
    [Fact(DisplayName = "Application key settings byte conversion")]
    public void ApplicationKeySettingsPreservesIdentity()
    {
        ApplicationKeySettings expected = new()
        {
            ChangeKey = ChangeKey.AnyApplicationKey(),
            KeySettingsReadOnly = true,
            MasterKeyReadOnly = false,
            FreeDirectoryListing = true,
            AllowCreateAndDeleteWithoutMasterKey = true,
        };

        ApplicationKeySettings actual = (ApplicationKeySettings)(byte)expected;

        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "Application key settings byte conversion (0xE5)")]
    public void ApplicationKeySettingsTestByte()
    {
        ApplicationKeySettings expected = new()
        {
            ChangeKey = ChangeKey.AnyApplicationKey(),
            KeySettingsReadOnly = true,
            MasterKeyReadOnly = false,
            FreeDirectoryListing = false,
            AllowCreateAndDeleteWithoutMasterKey = true,
        };

        Assert.Equal(0xE5, (byte)expected);
    }

    [Fact(DisplayName = "Application key settings byte conversion (0x1F)")]
    public void ApplicationKeySettingsTestByte2()
    {
        ApplicationKeySettings expected = new()
        {
            ChangeKey = ChangeKey.SpecificKey(1),
            KeySettingsReadOnly = false,
            MasterKeyReadOnly = false,
            FreeDirectoryListing = true,
            AllowCreateAndDeleteWithoutMasterKey = true,
        };

        Assert.Equal(0x1F, (byte)expected);
    }

    [Fact(DisplayName = "Application key settings byte conversion (0x02)")]
    public void ApplicationKeySettingsTestByte3()
    {
        ApplicationKeySettings expected = new()
        {
            ChangeKey = ChangeKey.SpecificKey(0),
            KeySettingsReadOnly = true,
            MasterKeyReadOnly = true,
            FreeDirectoryListing = true,
            AllowCreateAndDeleteWithoutMasterKey = false,
        };

        Assert.Equal(0x02, (byte)expected);
    }
}
