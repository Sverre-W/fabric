using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class ChangeKeyOperation(KeyGroupData oldKeyGroup, KeyGroupData newKeyGroup, int keySet, int keyNumber, byte? version) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        DesfireKeyId keyId = DesfireKeyId.Number((uint)keyNumber);

        byte[] oldKeyData = ChipDesignTransformer.CalculateKey(state, oldKeyGroup, keySet, keyId);
        byte[] newKeyData = ChipDesignTransformer.CalculateKey(state, newKeyGroup, keySet, keyId);

        return reader.ChangeKey(newKeyGroup.KeyType, keyId, oldKeyData, newKeyData, version, cancellationToken);
    }

    public override string ToString()
    {
        return $"Change {newKeyGroup.KeyType} key {keyNumber}";
    }
}
