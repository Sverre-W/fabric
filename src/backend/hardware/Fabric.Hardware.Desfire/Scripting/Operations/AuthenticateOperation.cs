using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;
using Fabric.Hardware.Desfire.Utils;
using KeyType = Fabric.Hardware.Desfire.Protocol.KeyType;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class AuthenticateOperation(KeyGroupData keyGroup, int keySet, int keyNumber) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        DesfireKeyId keyId = DesfireKeyId.Number((uint)keyNumber);
        byte[] keyData = ChipDesignTransformer.CalculateKey(state, keyGroup, keySet, keyId);

        return keyGroup.KeyType switch
        {
            KeyType.Aes => reader.AuthenticateAes(keyId, keyData, null, cancellationToken),
            KeyType.TDes => reader.Authenticate(keyId, keyData, keyGroup.KeyType, null, cancellationToken),
            _ => reader.AuthenticateIso(keyId, keyData, keyGroup.KeyType, null, cancellationToken),
        };
    }

    public override string ToString()
    {
        string authMethod = keyGroup.KeyType == KeyType.Aes ? "AES" : keyGroup.KeyType.ToString();
        int keyLength = CryptoHelper.GetKeySize(keyGroup.KeyType);
        return $"Authenticate {authMethod} with key {keyNumber} ({keyLength} bytes)";
    }
}

public class AuthenticateDefaultOperation(KeyGroupData keyGroup, int keyNumber, int keySet = 0) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        DesfireKeyId keyId = DesfireKeyId.Number((uint)keyNumber);
        byte[] keyData = ChipDesignTransformer.CalculateKey(state, keyGroup, keySet, keyId);

        return keyGroup.KeyType switch
        {
            KeyType.Aes => reader.AuthenticateAes(keyId, keyData, null, cancellationToken),
            KeyType.TDes => reader.Authenticate(keyId, keyData, keyGroup.KeyType, null, cancellationToken),
            _ => reader.AuthenticateIso(keyId, keyData, keyGroup.KeyType, null, cancellationToken),
        };
    }

    public override string ToString()
    {
        string authMethod = keyGroup.KeyType switch
        {
            KeyType.Aes => "AES",
            _ => keyGroup.KeyType.ToString(),
        };
        int keyLength = CryptoHelper.GetKeySize(keyGroup.KeyType);
        return $"Authenticate Default {authMethod} with key {keyNumber} ({keyLength} bytes)";
    }
}

public class AuthenticateDefaultProbeOperation(int keyNumber = 0) : IDesfireOperation
{
    private static readonly KeyType[] ProbeOrder = [KeyType.Aes, KeyType.Tdes2K, KeyType.Tdes3K, KeyType.TDes];

    public async Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        IDesfireResponse? lastResponse = null;

        foreach (KeyType keyType in ProbeOrder)
        {
            byte[] defaultKey = new byte[CryptoHelper.GetKeySize(keyType)];
            DesfireKeyId keyId = DesfireKeyId.Number((uint)keyNumber);

            lastResponse = keyType switch
            {
                KeyType.Aes => await reader.AuthenticateAes(keyId, defaultKey, ct: cancellationToken),
                KeyType.TDes => await reader.Authenticate(keyId, defaultKey, keyType, ct: cancellationToken),
                _ => await reader.AuthenticateIso(keyId, defaultKey, keyType, ct: cancellationToken),
            };

            if (lastResponse.IsSuccess)
            {
                return lastResponse;
            }
        }

        return lastResponse ?? DesfireResponse.Create(Fabric.Hardware.Desfire.Protocol.DesfireStatusCode.AuthenticationError);
    }

    public override string ToString()
    {
        return $"Probe default PICC key ({string.Join(", ", ProbeOrder.Select(x => x == KeyType.Aes ? "AES" : x.ToString()))})";
    }
}
