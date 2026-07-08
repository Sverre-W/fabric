using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class SelectApplicationOperation(DesfireApplicationId id) : IDesfireOperation
{
    public async Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        IDesfireResponse result = await reader.SelectApplication(id, cancellationToken);
        if (result.IsSuccess)
        {
            state.SelectedApplication = id.ToString();
        }

        return result;
    }

    public override string ToString()
    {
        return $"Select {id}";
    }
}
