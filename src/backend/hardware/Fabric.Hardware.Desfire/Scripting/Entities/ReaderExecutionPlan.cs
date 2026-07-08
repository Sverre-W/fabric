using Microsoft.Extensions.Logging;
using Fabric.Hardware.Desfire.Contracts;
using Fabric.Hardware.Desfire.Encoding.Models;
using Fabric.Hardware.Desfire.Scripting.Contracts;
using Fabric.Hardware.Desfire.Scripting.Operations;
using Fabric.Hardware.Desfire.Scripting.Services;
using Fabric.Hardware.Desfire.Services;

namespace Fabric.Hardware.Desfire.Scripting.Entities;

public class ReaderExecutionPlan(Dictionary<ReaderProfile, List<IDesfireOperation>> operations)
    : IExecutionPlan<ReaderExecutedPlan>
{
    public IReadOnlyList<IDesfireOperation> Operations => [.. operations.SelectMany(x => x.Value)];

    public IReadOnlyList<string> RequiredProviders => [];

    public event DesfireCommandEventHandler? OnCommandExecuted;

    public async Task<ReaderExecutedPlan> Execute(
        ILogger logger,
        Dictionary<string, byte[]> variables,
        IRfidEncoder encoder,
        CancellationToken cancellationToken = default
    )
    {
        DesfireReader reader = new(logger, encoder, false);
        ReaderExecutedPlan executedPlan = new();
        foreach (var readerProfileKeyValue in operations)
        {
            ExecutionState state = new() { Variables = variables };

            string? lastVariable = null;
            bool isSuccess = true;
            int index = 0;
            foreach (var operation in readerProfileKeyValue.Value)
            {
                // Execute the operation using the encoder
                var response = await operation.Execute(state, reader, cancellationToken);

                // Trigger the event after each command execution
                executedPlan.AddResult(operation, response);
                OnCommandExecuted?.Invoke(this, new DesfireEventArgs(index++, operation, response, reader, state.CardUid));
                isSuccess &= response.IsSuccess;

                if (operation is ReadFromFileOperation command)
                {
                    lastVariable = command.Variable;
                }

                if (!response.IsSuccess)
                    break;
            }

            if (isSuccess && lastVariable != null)
            {
                byte[] lastVariableProvider = state.Variables[lastVariable];
                executedPlan.ReadData = lastVariableProvider;
                executedPlan.ReaderProfile = readerProfileKeyValue.Key;
                break;
            }
        }

        return executedPlan;
    }

    async Task<object?> IExecutionPlan.Execute(
        ILogger logger,
        Dictionary<string, byte[]> variables,
        IRfidEncoder encoder,
        CancellationToken cancellationToken
    )
    {
        return await Execute(logger, variables, encoder, cancellationToken);
    }
}

public class ReaderExecutedPlan : IExecutedPlan
{
    public IReadOnlyList<(IDesfireOperation, IDesfireResponse)> Operations =>
        _operations.AsReadOnly();

    public List<(IDesfireOperation, IDesfireResponse)> _operations = [];

    public ReaderProfile? ReaderProfile { get; set; }
    public byte[]? ReadData { get; set; }

    internal void AddResult(IDesfireOperation operation, IDesfireResponse response)
    {
        _operations.Add((operation, response));
    }
}
