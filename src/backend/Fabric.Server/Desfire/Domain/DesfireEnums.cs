namespace Fabric.Server.Desfire.Domain;

public enum EncodingRunKind
{
    Single,
    Batch
}

public static class DesfireEncodingSources
{
    public const string Kiosk = "kiosk";
    public const string PrintBatch = "print-batch";
}

public enum EncodingRunStatus
{
    Pending,
    Claimed,
    Running,
    Succeeded,
    Failed,
    Cancelled,
    Timeout,
    DeviceUnavailable
}

public enum AdHocEncodingMode
{
    Sync,
    Queued
}

public enum DesfireVariableProviderKind
{
    Provided,
    Fixed,
    Sequence
}

public enum DesfireVariableFormatKind
{
    Hex,
    Text,
    UInt,
    PaddedDecimal,
    PaddedHex,
    GenericWiegand
}

public enum WiegandParityKind
{
    Even,
    Odd
}

public enum WiegandFieldSourceKind
{
    Provided,
    Fixed,
    Sequence
}

public enum EncodingBatchStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum KeyGroupError
{
    AlreadyLocked,
    CannotEditLocked,
    CannotChangeKeyStructure,
    DiversifiedKeyRequiresStrategy,
    EmptyKeySets
}

public enum TransformationVariableKind
{
    UserProvided,
    SystemProvided
}

public enum SystemVariableProviderKind
{
    Fixed,
    Sequence
}
