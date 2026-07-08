using Microsoft.Extensions.Logging;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Contracts;

public interface IExecutionPlan
{
    /// <summary>
    /// Gets the list of Desfire operations to be executed.
    /// </summary>
    IReadOnlyList<IDesfireOperation> Operations { get; }

    /// <summary>
    /// Gets the list of required variables for the execution plan.
    /// </summary>
    IReadOnlyList<string> RequiredProviders { get; }

    /// <summary>
    /// Event triggered when a Desfire command is executed.
    /// </summary>
    event DesfireCommandEventHandler? OnCommandExecuted;

    /// <summary>
    /// Executes the plan with the provided variable providers and encoder.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="variables">A dictionary of variable providers.</param>
    /// <param name="encoder">The RFID encoder to use for execution.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous execution of the plan.</returns>
    Task<object?> Execute(
        ILogger logger,
        Dictionary<string, byte[]> variables,
        IRfidEncoder encoder,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Represents an execution plan for Desfire operations.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the execution plan.</typeparam>
public interface IExecutionPlan<TResult> : IExecutionPlan
    where TResult : IExecutedPlan
{
    /// <summary>
    /// Executes the plan with the provided variable providers and encoder.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="variables">A dictionary of variable providers.</param>
    /// <param name="encoder">The RFID encoder to use for execution.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous execution of the plan.</returns>
    new Task<TResult> Execute(
        ILogger logger,
        Dictionary<string, byte[]> variables,
        IRfidEncoder encoder,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Delegate for handling Desfire command execution events.
/// </summary>
/// <param name="sender">The source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void DesfireCommandEventHandler(object? sender, DesfireEventArgs e);

/// <summary>
/// Provides data for Desfire command execution events.
/// </summary>
public class DesfireEventArgs(
    int index,
    IDesfireOperation operation,
    IDesfireResponse response,
    DesfireReader reader,
    string? cardUid = null
)
    : EventArgs
{
    /// <summary>
    /// The index of the executed command within the execution plan.
    /// </summary>
    public int Index { get; } = index;

    /// <summary>
    /// Gets the Desfire operation that was executed.
    /// </summary>
    public IDesfireOperation Operation { get; } = operation;

    /// <summary>
    /// Gets the response from the executed Desfire operation.
    /// </summary>
    public IDesfireResponse Response { get; } = response;

    /// <summary>
    /// Gets the live reader used for the executed operation.
    /// </summary>
    public DesfireReader Reader { get; } = reader;

    /// <summary>
    /// Gets the current card UID if one is known after the operation.
    /// </summary>
    public string? CardUid { get; } = cardUid;
}
