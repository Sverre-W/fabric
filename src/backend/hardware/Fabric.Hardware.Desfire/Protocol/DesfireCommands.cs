namespace Fabric.Hardware.Desfire.Protocol;

public enum DesfireCommand
{
    GetVersion = 0x60,
    NextFrame = 0xAF,
    Authenticate = 0x0A,
    AuthenticateIso = 0x1A,
    AuthenticateAes = 0xAA,
    AuthenticateEv2First = 0x71,
    AuthenticateEv2NotFirst = 0x77,
    GetCardUid = 0x51,
    GetFreeMemory = 0x6E,
    GetApplicationIds = 0x6A,
    SelectApplication = 0x5A,
    CreateApplication = 0xCA,
    DeleteApplication = 0xDA,
    SetConfiguration = 0x5C,
    GetKeySettings = 0x45,
    ChangeKey = 0xC4,
    ChangeKeySettings = 0x54,
    ChangeFileSettings = 0x5F,
    CreateStandardFile = 0xCD,
    DeleteFile = 0xDF,
    GetFileIds = 0x6F,
    WriteData = 0x3D,
    ReadData = 0xBD,
    Format = 0xFC,
}
