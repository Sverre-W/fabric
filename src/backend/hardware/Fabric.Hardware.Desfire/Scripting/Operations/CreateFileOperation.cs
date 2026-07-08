using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class CreateFileOperation(DesfireFile file) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.CreateFile(file, cancellationToken);
    }

    public override string ToString()
    {
        return $"Create File {file.FileNumber} of {file.FileSize} bytes (R: {file.AccessRights.ReadKey} W: {file.AccessRights.WriteKey} RW: {file.AccessRights.ReadWriteKey} C: {file.AccessRights.ChangeKey})";
    }
}
