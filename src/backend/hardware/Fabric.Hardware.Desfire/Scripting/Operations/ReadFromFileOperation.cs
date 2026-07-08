using Microsoft.Extensions.Logging;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class ReadFromFileOperation(
    int fileId,
    CommunicationMode mode,
    int offset,
    int size,
    string variable
) : IDesfireOperation
{
    public string Variable => variable;

    public async Task<IDesfireResponse> Execute(
        ExecutionState state,
        DesfireReader reader,
        CancellationToken cancellationToken = default
    )
    {
        IDesfireResponse<byte[]> result = await reader.ReadData(
            fileId,
            offset,
            size,
            mode,
            cancellationToken
        );

        if (result.IsSuccess)
        {
            state.Variables[variable] = result.Data ?? throw new Exception("Could not read data");
            reader.Logger.LogDebug(
                "Read {Data} and stored into {Variable}",
                Convert.ToHexString(result.Data!),
                variable
            );
        }

        return result;
    }

    public override string ToString()
    {
        return $"Read file {fileId} and store in variable {variable} ({mode})";
    }
}
