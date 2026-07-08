using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Services;

namespace Fabric.Hardware.Desfire.Scripting.Contracts;

/// <summary>
/// Represents the result of an executed plan.
/// </summary>
public interface IExecutedPlan
{
    /// <summary>
    /// Gets or sets the list of operations and their corresponding responses.
    /// </summary>
    public IReadOnlyList<(IDesfireOperation, IDesfireResponse)> Operations { get; }
}
