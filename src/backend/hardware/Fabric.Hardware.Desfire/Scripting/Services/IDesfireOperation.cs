using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Entities;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Services;

public interface IDesfireOperation
{
    Task<IDesfireResponse> Execute(ExecutionState state, DesfireReader reader, CancellationToken cancellationToken = default);
}
