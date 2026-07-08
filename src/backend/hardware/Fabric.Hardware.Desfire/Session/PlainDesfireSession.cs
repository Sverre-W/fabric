using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Desfire.Session;

/// <summary>
///     The starting DESFire Session. No Authentication is active
/// </summary>
public class PlainDesfireSession : DesfireSession
{
    public PlainDesfireSession(ILogger logger, IRfidEncoder cardEncoder)
        : base(logger, cardEncoder)
    {
        KeyId = 0x00;
    }

    public override KeyType KeyType => KeyType.None;

    protected override byte[] PreProcessEncrypt(DesfireCommandFrame command)
    {
        return command.CalculateApdu();
    }

    protected override byte[] PreProcessCmaced(DesfireCommandFrame command)
    {
        return command.CalculateApdu();
    }

    protected override byte[] PreProcessPlain(DesfireCommandFrame command)
    {
        return command.CalculateApdu();
    }

    protected override byte[] PostProcess(CommunicationMode mode, byte[] response, int? length)
    {
        return response;
    }
}
