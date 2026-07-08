using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class SecureMessagingChangeOperation(SecureMessingConfiguration settings) : IDesfireOperation
{
    public async Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        var config = CardConfiguration.SetSecureMessaging(settings.DisableEv2Chaining, settings.DisableEv1, settings.DisableD40);

        return await reader.ChangeConfiguration(config, cancellationToken);
    }

    public override string ToString()
    {
        return $"Change Secure Messaging settings: DisableEv2Chaining={settings.DisableEv2Chaining}, DisableEv1={settings.DisableEv1}, DisableD40={settings.DisableD40}";
    }
}
