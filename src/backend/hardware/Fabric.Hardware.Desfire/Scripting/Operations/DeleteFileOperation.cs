using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class DeleteFileOperation(int fileId) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState _, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.DeleteFile(fileId, cancellationToken);
    }

    public override string ToString()
    {
        return $"Delete file {fileId}";
    }
}
