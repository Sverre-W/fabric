using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class GetVersionOperation : IDesfireOperation
{
    public async Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        IDesfireResponse<DesfireVersion> result = await reader.GetVersion(cancellationToken);

        if (result is { IsSuccess: true, Data: not null })
        {
            state.CardUid = result.Data.CardId;
        }

        return result;
    }

    public override string ToString()
    {
        return "Get Card Version";
    }
}
