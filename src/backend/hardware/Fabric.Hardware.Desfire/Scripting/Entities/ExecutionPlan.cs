using Microsoft.Extensions.Logging;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Scripting.Contracts;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Protocol;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Entities;

/// <summary>
/// </summary>
/// <param name="operations"></param>
/// <param name="requiredProviders"></param>
/// <param name="errors"></param>
public class ExecutionPlan(List<IDesfireOperation> operations, List<string> requiredProviders, List<ScriptError> errors)
    : IExecutionPlan<ExecutedEncodingPlan>
{
    /// <summary>
    ///     The operations that have to be executed
    /// </summary>
    public IReadOnlyList<IDesfireOperation> Operations => [.. operations];

    /// <summary>
    ///     Variables that have to be provided on execution. This excludes any variables obtained from the
    ///     card
    /// </summary>
    public IReadOnlyList<string> RequiredProviders => requiredProviders;

    /// <summary>
    /// An event triggered after each command execution
    /// </summary>
    public event DesfireCommandEventHandler? OnCommandExecuted;

    /// <summary>
    ///     A list of errors with for the current transformation
    /// </summary>
    public List<ScriptError> Errors { get; init; } = errors;

    /// <summary>
    ///     Execute the current plan against an RFID card
    /// </summary>
    /// <param name="variableProviders">A dictionary with a provider for each required variable</param>
    /// <param name="encoder">The RFID encoder to be used</param>
    /// <param name="operationExecuted">
    ///     A function to be executed after each desfire operation
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ExecutedEncodingPlan> Execute(
        ILogger logger,
        Dictionary<string, byte[]> variables,
        IRfidEncoder encoder,
        CancellationToken cancellationToken = default
    )
    {
        ExecutedEncodingPlan executedPlan = new();
        DesfireReader reader = new(logger, encoder, false);
        executedPlan.Variables = variables;

        ExecutionState state = new() { Variables = variables };
        int index = 0;
        foreach (IDesfireOperation op in operations)
        {
            try
            {
                string selectedApplication = state.SelectedApplication;
                logger.LogDebug("Executing op: {op}", op.ToString());
                IDesfireResponse result = await op.Execute(state, reader, cancellationToken);
                executedPlan.AddResult(op, result);

                OnCommandExecuted?.Invoke(this, new DesfireEventArgs(index++, op, result, reader, state.CardUid));

                if (!result.IsSuccess)
                {
                    executedPlan.IsSuccess = false;
                    executedPlan.ErrorMessage = $"[{index}/{operations.Count}] {op}: {result.StatusCode.Describe()} (selected {selectedApplication})";
                    break;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to execute operation {operation}", op);
                executedPlan.CardUid = state.CardUid;
                executedPlan.SelectedApplication = state.SelectedApplication;
                executedPlan.IsSuccess = false;
                executedPlan.ErrorMessage = ex.Message;
                break;
            }
        }

        executedPlan.CardUid = state.CardUid;
        executedPlan.SelectedApplication = state.SelectedApplication;

        return executedPlan;
    }

    async Task<object?> IExecutionPlan.Execute(
        ILogger logger,
        Dictionary<string, byte[]> variableProviders,
        IRfidEncoder encoder,
        CancellationToken cancellationToken
    )
    {
        return await Execute(logger, variableProviders, encoder, cancellationToken);
    }
}

public class ExecutedEncodingPlan : IExecutedPlan
{
    /// <summary>
    ///     Executed operations and their respective results
    /// </summary>
    public IReadOnlyList<(IDesfireOperation, IDesfireResponse)> Operations => _operations.AsReadOnly();

    private readonly List<(IDesfireOperation, IDesfireResponse)> _operations = [];

    /// <summary>
    ///     Represent the current select application
    /// </summary>
    internal string SelectedApplication { get; set; } = "000000";

    /// <summary>
    ///     The Card UID
    /// </summary>
    public string CardUid { get; set; } = "Unknown";

    /// <summary>
    ///     Evaluated variables
    /// </summary>
    public Dictionary<string, byte[]> Variables { get; set; } = [];

    public bool IsSuccess { get; set; } = true;

    public string? ErrorMessage { get; set; }

    internal void AddResult(IDesfireOperation operation, IDesfireResponse response)
    {
        _operations.Add((operation, response));
    }
}
