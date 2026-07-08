namespace Fabric.Hardware.Desfire.Models;

public class VersionInfo
{
    public VersionInfo(byte[] data)
    {
        if (data.Length < 7)
        {
            throw new ArgumentException("Version info must contain at least 7 bytes", nameof(data));
        }

        string hexStr = Convert.ToHexString(data);
        VendorId = hexStr[..2];
        Type = hexStr[2..4];
        SubByte = hexStr[4..6];
        MajorVersion = hexStr[6..8];
        MinorVersion = hexStr[8..10];
        StorageSize = hexStr[10..12];
        ProtocolType = hexStr[12..14];
    }

    public string VendorId { get; init; }
    public string Type { get; init; }
    public string SubByte { get; init; }
    public string MajorVersion { get; init; }
    public string MinorVersion { get; init; }
    public string StorageSize { get; init; }
    public string ProtocolType { get; init; }
}
