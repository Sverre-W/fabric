using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class DeleteApplicationOperation(DesfireApplicationId id) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.DeleteApplication(id, cancellationToken);
    }

    public override string ToString()
    {
        return $"Delete application {id}";
    }
}
