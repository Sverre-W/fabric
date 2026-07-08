using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Models;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Operations;

public class CreateApplicationOperation(ApplicationDescription description) : IDesfireOperation
{
    public Task<IDesfireResponse> Execute(ExecutionState _, DesfireReader reader, CancellationToken cancellationToken = default)
    {
        return reader.CreateApplication(description, cancellationToken);
    }

    public override string ToString()
    {
        return $"Create application {description}";
    }
}
