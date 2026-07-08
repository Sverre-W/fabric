namespace Fabric.Hardware.Desfire.Protocol;

public enum DesfireStatusCode
{
    Success = 0x00,
    SuccessLimitedFunctionality = 0x01,
    NoChanges = 0x0C,
    OutOfMemory = 0x0E,
    IllegalCommandCode = 0x1C,
    IntegrityError = 0x1E,
    NoSuchKey = 0x40,
    LengthError = 0x7E,
    PermissionDenied = 0x9D,
    ParameterError = 0x9E,
    ApplicationNotFound = 0xA0,
    AuthenticationError = 0xAE,
    AdditionalFrame = 0xAF,
    BoundaryError = 0xBE,
    CommandAborted = 0xCA,
    DuplicateError = 0xDE,
    MemoryError = 0xEE,
    FileNotFound = 0xF0,
}
