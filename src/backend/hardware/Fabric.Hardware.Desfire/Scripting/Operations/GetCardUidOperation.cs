using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class GetCardUidOperation : IDesfireOperation
{
    public async Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        IDesfireResponse<string> result = await reader.GetCardUid(cancellationToken);

        if (result.IsSuccess)
        {
            state.CardUid = result.Data!;
        }

        return result;
    }

    public override string ToString()
    {
        return "Get card UID";
    }
}
