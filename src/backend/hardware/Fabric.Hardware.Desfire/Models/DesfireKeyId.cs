namespace Fabric.Hardware.Desfire.Models;

public record DesfireKeyId
{
    public static readonly DesfireKeyId ApplicationMasterKey = new(0x00);
    public static readonly DesfireKeyId PiccMasterKey = ApplicationMasterKey;
    private readonly byte _keyId;

    private DesfireKeyId(uint keyId)
    {
        _keyId = (byte)keyId;
    }

    public static DesfireKeyId Number(uint number)
    {
        return new DesfireKeyId(number);
    }

    public static implicit operator byte(DesfireKeyId keyId)
    {
        return keyId._keyId;
    }

    public override string ToString()
    {
        return ((int)_keyId).ToString();
    }
}
