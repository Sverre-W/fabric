using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Encoding.Specifications;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class ChangePiccSettingsOperation(PiccSettings settings) : IDesfireOperation
{
    public async Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        CardConfiguration config = CardConfiguration.SetPiccConfig(
            settings.DisableCardFormat,
            settings.RandomIdEnabled,
            settings.ProximityCheckMandatory,
            settings.IsoVirtualCardMandatory,
            settings.EnableLegacyRandomId
        );

        return await reader.ChangeConfiguration(config, cancellationToken);
    }

    public override string ToString()
    {
        List<string> flags = [];

        if (settings.DisableCardFormat)
            flags.Add("Disable Card Format");

        if (settings.ProximityCheckMandatory)
            flags.Add("Proximity Check Mandatory");

        if (settings.RandomIdEnabled)
            flags.Add(settings.EnableLegacyRandomId ? "Random ID Enabled (Legacy)" : "Random ID Enabled");

        if (settings.IsoVirtualCardMandatory)
            flags.Add("ISO Virtual Card Mandatory");

        return $"Change PICC settings: {string.Join(", ", flags)}";
    }
}
