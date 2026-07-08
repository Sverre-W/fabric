namespace Fabric.Hardware.Desfire.Models;

public class ApplicationDescription
{
    private readonly byte[] _createApplicationHeader = new byte[29];

    private ApplicationExtendedSettings? _applicationExtendedSettings;
    private ApplicationKeySettings _applicationKeySettings;
    private ApplicationSettings _applicationSettings;
    private int _headerLength = 5;

    private DesfireApplicationId appId = null!;

    private ApplicationDescription()
    {
        _applicationSettings = new ApplicationSettings();
        _applicationKeySettings = new ApplicationKeySettings();
    }

    public static ApplicationDescription NewApplication(DesfireApplicationId applicationId)
    {
        return new ApplicationDescription().ApplicationId(applicationId);
    }

    public ApplicationDescription ApplicationId(DesfireApplicationId applicationId)
    {
        this.appId = applicationId;
        byte[] appId = applicationId.AsBytes();
        Array.Copy(appId, 0, _createApplicationHeader, 0, appId.Length);
        return this;
    }

    public ApplicationDescription KeySettings(ApplicationKeySettings applicationKeySettings)
    {
        _applicationKeySettings = applicationKeySettings;
        return this;
    }

    public ApplicationDescription Settings(ApplicationSettings applicationSettings)
    {
        _applicationSettings = applicationSettings;
        return this;
    }

    public ApplicationDescription ExtendedSettings(ApplicationExtendedSettings? applicationSettings)
    {
        _applicationExtendedSettings = applicationSettings;

        if (_applicationExtendedSettings != null)
        {
            MinimumExpectedHeaderLength(6);
            _applicationSettings.ExtendedApplicationSettings = true;
        }
        else
        {
            MinimumExpectedHeaderLength(5);
            _applicationSettings.ExtendedApplicationSettings = false;
        }

        return this;
    }

    public ApplicationDescription ActiveKeySetVersion(ushort keyVersion)
    {
        MinimumExpectedHeaderLength(7);
        _createApplicationHeader[6] = (byte)keyVersion;
        return this;
    }

    public ApplicationDescription KeySets(ushort numberOfKeySets)
    {
        if (numberOfKeySets == 1)
            return this;

        if (numberOfKeySets is < 2 or > 16)
        {
            throw new ArgumentException("Number of key Sets must be between 2 and 16");
        }

        MinimumExpectedHeaderLength(8);
        _createApplicationHeader[7] = (byte)numberOfKeySets;
        return this;
    }

    public ApplicationDescription Only16ByteKeys()
    {
        MinimumExpectedHeaderLength(9);
        _createApplicationHeader[8] = 0x10;
        return this;
    }

    public ApplicationDescription AnyKey()
    {
        MinimumExpectedHeaderLength(9);
        _createApplicationHeader[8] = 0x18;
        return this;
    }

    public ApplicationDescription KeySetKeySettings(ApplicationKeySettings applicationKeySettings)
    {
        MinimumExpectedHeaderLength(10);
        _createApplicationHeader[9] = (byte)applicationKeySettings;
        return this;
    }

    public ApplicationDescription IsoFileId(byte[] isoFileId)
    {
        if (isoFileId.Length != 2)
        {
            throw new ArgumentException("Invalid ISO file ID");
        }

        MinimumExpectedHeaderLength(12);
        Array.Copy(isoFileId, 0, _createApplicationHeader, 10, isoFileId.Length);
        return this;
    }

    public ApplicationDescription IsoDfName(byte[] name)
    {
        if (name.Length is < 1 or > 16)
        {
            throw new ArgumentException("Invalid ISO DF Name");
        }

        MinimumExpectedHeaderLength(12 + name.Length);
        Array.Copy(name, 0, _createApplicationHeader, 12, name.Length);
        return this;
    }

    internal byte[] Build()
    {
        _createApplicationHeader[3] = (byte)_applicationKeySettings;
        _createApplicationHeader[4] = (byte)_applicationSettings;

        if (_applicationExtendedSettings != null)
        {
            _createApplicationHeader[5] = (byte)_applicationExtendedSettings;
        }

        return _createApplicationHeader[.._headerLength];
    }

    private void MinimumExpectedHeaderLength(int length)
    {
        _headerLength = length < _headerLength ? _headerLength : length;
    }

    public override string ToString()
    {
        return $"{appId}";
    }
}
