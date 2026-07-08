using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class ChangePiccKeyOperation(KeyGroupData keyGroup, int keySet, int keyNumber, byte version) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        byte[] keyData = ChipDesignTransformer.CalculateKey(state, keyGroup, keySet, keyNumber);
        return reader.ChangePiccKey(keyGroup.KeyType, keyData, version, cancellationToken);
    }

    public override string ToString()
    {
        int keyLength = keyGroup.KeySets[keySet].Keys[keyNumber].Value.Length / 2;
        return $"Change PICC Key [{keyGroup.KeyType}, {keyLength} bytes]";
    }
}
