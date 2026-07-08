using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class ChangeApplicationKeySettingsOperation(ApplicationKeySettings keySettings) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState _, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.ChangeKeySettings(keySettings, cancellationToken);
    }

    public override string ToString()
    {
        return "Change application key settings";
    }
}
