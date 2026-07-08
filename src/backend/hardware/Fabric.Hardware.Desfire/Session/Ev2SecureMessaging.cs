using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Desfire.Session;

public class Ev2SecureMessaging : DesfireSession
{
    public Ev2SecureMessaging(
        ILogger logger,
        IRfidEncoder cardEncoder,
        byte keyId,
        byte[] transactionId,
        byte[] sessionKey,
        byte[] macingKey,
        ushort commandCounter = 0
    )
        : base(logger, cardEncoder)
    {
        KeyType = KeyType.Aes;
        KeyId = keyId;
        CommandCounter = commandCounter;
        TransactionId = transactionId;
        SessionKey = sessionKey;
        MacingKey = macingKey;
    }

    public override KeyType KeyType { get; }
    public byte[] SessionKey { get; }
    public byte[] MacingKey { get; }
    public byte[] TransactionId { get; }

    public ushort CommandCounter { get; }

    protected override byte[] PreProcessEncrypt(DesfireCommandFrame command)
    {
        throw new NotImplementedException();
    }

    protected override byte[] PreProcessCmaced(DesfireCommandFrame command)
    {
        throw new NotImplementedException();
    }

    protected override byte[] PreProcessPlain(DesfireCommandFrame command)
    {
        throw new NotImplementedException();
    }

    protected override byte[] PostProcess(CommunicationMode mode, byte[] data, int? length)
    {
        throw new NotImplementedException();
    }
}
