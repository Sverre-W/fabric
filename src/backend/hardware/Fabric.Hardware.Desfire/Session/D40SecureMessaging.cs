using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Utils;
using Microsoft.Extensions.Logging;

namespace Fabric.Hardware.Desfire.Session;

public class D40SecureMessaging : DesfireSession
{
    public readonly byte[] _iv;

    public D40SecureMessaging(ILogger logger, IRfidEncoder cardEncoder, KeyType keyType, byte keyId, byte[] key)
        : base(logger, cardEncoder)
    {
        KeyType = keyType;
        SessionKey = key;
        KeyId = keyId;
        _iv = new byte[CryptoHelper.GetBlockSize(keyType)];
    }

    public byte[] SessionKey { get; }
    public override KeyType KeyType { get; }

    protected override byte[] PreProcessEncrypt(DesfireCommandFrame command)
    {
        byte[] encrypted = command.EncryptD40(KeyType, _iv, SessionKey);
        command.Data = encrypted;
        return command.CalculateApdu();
    }

    protected override byte[] PreProcessCmaced(DesfireCommandFrame command)
    {
        //The first 4 bytes of the last encrypted block
        byte[] mac = CalculateMac(command.Data);

        byte[] fullData = new byte[command.Data.Length + mac.Length];
        Array.Copy(command.Data, 0, fullData, 0, command.Data.Length);
        Array.Copy(mac, 0, fullData, command.Data.Length, mac.Length);

        command.Data = fullData;
        return command.CalculateApdu();
    }

    protected override byte[] PreProcessPlain(DesfireCommandFrame command)
    {
        return command.CalculateApdu();
    }

    protected override byte[] PostProcess(CommunicationMode mode, byte[] data, int? length)
    {
        if (mode is CommunicationMode.Plain)
        {
            return data;
        }

        if (mode is CommunicationMode.Cmac)
        {
            VerifyCmac(data[1..]);
            //We remove the last 4 bytes from the response which is the MAC we received
            return data[..^4];
        }

        if (length == null)
        {
            throw new NotSupportedException("Length required for decryption");
        }

        byte[] plainText = CryptoHelper.DecryptDesfireNative(KeyType, SessionKey, data[1..]);
        byte[] payload = plainText[..length.Value];

        byte[] decryptedResponse = new byte[payload.Length + 1];
        decryptedResponse[0] = data[0];
        Array.Copy(payload, 0, decryptedResponse, 1, payload.Length);
        return decryptedResponse;
    }

    private byte[] CalculateMac(byte[] data)
    {
        //Ensure that we pad the data
        int blockSize = CryptoHelper.GetBlockSize(KeyType);

        int paddingRequired = blockSize - (data.Length % blockSize);
        paddingRequired = paddingRequired == blockSize ? 0 : paddingRequired;

        byte[] paddedData = new byte[data.Length + paddingRequired];

        //Add Data
        Array.Copy(data, paddedData, data.Length);
        byte[] encrypted = CryptoHelper.Encrypt(KeyType, _iv, SessionKey, paddedData);

        //The first 4 bytes of the last encrypted block
        return encrypted[^blockSize..][..4];
    }

    private void VerifyCmac(byte[] response)
    {
        byte[] receivedMac = response[^4..];
        byte[] macCalculated = CalculateMac(response[..^4]);

        //Verify the MAC we received matches the MAC we calculated
        for (int i = 0; i < 4; i++)
        {
            if (macCalculated[i] != receivedMac[i])
            {
                string message =
                    $"MAC does not match, Expected: {Convert.ToHexString(macCalculated[..4])} Actual: {Convert.ToHexString(receivedMac)}";
                throw new InvalidOperationException(message);
            }
        }
    }
}
