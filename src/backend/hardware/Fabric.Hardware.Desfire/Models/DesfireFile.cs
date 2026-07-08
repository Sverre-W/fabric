namespace Fabric.Hardware.Desfire.Models;

public abstract class DesfireFile
{
    protected DesfireFile(int fileNumber, DesfireFileOptions fileOptions, DesfireFileAccessRights accessRights, int fileSize, int isoFileId = 0)
    {
        FileNumber = fileNumber;
        IsoFileId = isoFileId;
        FileOptions = fileOptions;
        AccessRights = accessRights;
        FileSize = fileSize;
    }

    public int FileNumber { get; set; }
    public int IsoFileId { get; set; }
    public DesfireFileOptions FileOptions { get; set; }
    public DesfireFileAccessRights AccessRights { get; set; }
    public int FileSize { get; set; }

    public static StandardDesfireFile CreateStandardFile(
        int fileNumber,
        DesfireFileOptions fileOptions,
        DesfireFileAccessRights accessRights,
        int fileSize,
        int isoFileId
    )
    {
        return new StandardDesfireFile(fileNumber, fileOptions, accessRights, fileSize, isoFileId);
    }
}

public class StandardDesfireFile : DesfireFile
{
    public StandardDesfireFile(int fileNumber, DesfireFileOptions fileOptions, DesfireFileAccessRights accessRights, int fileSize, int isoFileId = 0)
        : base(fileNumber, fileOptions, accessRights, fileSize, isoFileId) { }
}
