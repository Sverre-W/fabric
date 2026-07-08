using Fabric.Hardware.Desfire.Encoding.Models;
using KeyType = Fabric.Hardware.Desfire.Protocol.KeyType;

namespace Fabric.Hardware.Desfire.Scripting.Services;

public class KeyGroupData
{
    public KeyType KeyType { get; set; }
    public KeySet[] KeySets { get; set; } = [];
    public KeyDiversificationStrategy? KeyDiversificationStrategy { get; set; }
}

public interface IKeyGroupResolver
{
    public Task<KeyGroupData?> ResolveKeyGroup(string keyGroupName, CancellationToken ct = default);
}
