using Microsoft.Extensions.Logging;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Scripting.Utilities;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class WriteToFileOperation(
    int fileId,
    CommunicationMode mode,
    string variable,
    string encoding,
    int offset = 0,
    int size = 0
) : IDesfireOperation
{
    public async Task<IDesfireResponse> Execute(
        ExecutionState state,
        DesfireReader reader,
        CancellationToken cancellationToken = default
    )
    {
        byte[] providedData = state.Variables[variable];
        byte[] encodedData = VariableEncodingUtilities.EncodeForFile(providedData, encoding);

        reader.Logger.LogDebug(
            "Write variable {variable} for file {file} using encoding {encoding}: raw={raw} encoded={encoded}",
            variable,
            fileId,
            encoding,
            Convert.ToHexString(providedData),
            Convert.ToHexString(encodedData)
        );

        if (encodedData.Length == 0)
        {
            reader.Logger.LogDebug("Skipping empty write for file {file} variable {variable}", fileId, variable);
            return DesfireResponse.Create(DesfireStatusCode.Success);
        }

        if (size > 0 && encodedData.Length > size)
        {
            reader.Logger.LogWarning(
                "Truncating data of {DataSize} bytes to fit file size {FileSize} bytes",
                encodedData.Length,
                size
            );

            encodedData = encodedData[..size];
        }

        reader.Logger.LogDebug(
            "Writing {Data} ({DataLength}) to File {file}",
            Convert.ToHexString(encodedData),
            encodedData.Length,
            fileId
        );

        return await reader.WriteData(fileId, encodedData, offset, encodedData.Length, mode, cancellationToken);
    }

    public override string ToString()
    {
        return $"Write variable {variable} to file {fileId} ({mode}, encoding={encoding})";
    }
}
