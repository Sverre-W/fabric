using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class FormatOperation : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState _, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.Format(cancellationToken);
    }

    public override string ToString()
    {
        return "Execute format";
    }
}
