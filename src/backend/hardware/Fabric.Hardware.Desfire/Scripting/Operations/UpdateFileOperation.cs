using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class UpdateFileOperation(int fileId, DesfireFileAccessRights accessRights, DesfireFileOptions options) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState _, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.ChangeFileSettings(fileId, options, accessRights, cancellationToken);
    }

    public override string ToString()
    {
        return $"Update file settings for file {fileId}";
    }
}
