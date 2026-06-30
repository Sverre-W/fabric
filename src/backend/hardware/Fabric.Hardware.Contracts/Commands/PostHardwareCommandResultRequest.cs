using System.Text.Json.Nodes;
using Fabric.Hardware.Contracts;

namespace Fabric.Hardware.Contracts.Commands;

public sealed record PostHardwareCommandResultRequest(
    HardwareOperationStatus Status,
    JsonObject? Result,
    HardwareErrorResponse? Error,
    DateTimeOffset CompletedAt);
