using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Fabric.Hardware.Contracts;
using Fabric.Hardware.Contracts.Agents;
using Fabric.Hardware.Contracts.Commands;
using Fabric.Hardware.Contracts.Events;
using Fabric.Hardware.Contracts.Inventory;
using Fabric.Hardware.Contracts.Labels;
using Fabric.Hardware.Contracts.Qr;
using Fabric.Server.Core;
using Fabric.Server.Hardware.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Hardware;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(HardwareDeviceRef))]
[JsonSerializable(typeof(HardwareOperationStatus))]
[JsonSerializable(typeof(HardwareCommandStatus))]
[JsonSerializable(typeof(HardwareCommandEnvelope))]
[JsonSerializable(typeof(HardwareCommandClaimResponse))]
[JsonSerializable(typeof(PostHardwareCommandResultRequest))]
[JsonSerializable(typeof(PostHardwareInventoryRequest))]
[JsonSerializable(typeof(HardwareDeviceInventoryItem))]
[JsonSerializable(typeof(HardwareDeviceDiagnostics))]
[JsonSerializable(typeof(PostHardwareEventRequest))]
[JsonSerializable(typeof(PostHardwareAgentHeartbeatRequest))]
[JsonSerializable(typeof(QrScanResponse))]
[JsonSerializable(typeof(PrintLabelRequest))]
[JsonSerializable(typeof(LabelPrintResponse))]
[JsonSerializable(typeof(HardwareAgentResponse))]
[JsonSerializable(typeof(CreateHardwareAgentRequest))]
[JsonSerializable(typeof(UpdateHardwareAgentRequest))]
[JsonSerializable(typeof(HardwareAgentKeyResponse))]
[JsonSerializable(typeof(HardwareDeviceResponse))]
[JsonSerializable(typeof(HardwareDeviceResponse[]))]
[JsonSerializable(typeof(HardwareDeviceHealthResponse))]
[JsonSerializable(typeof(IReadOnlyList<HardwareDeviceResponse>))]
[JsonSerializable(typeof(Page<HardwareAgentResponse>))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class HardwareJsonSerializerContext : JsonSerializerContext;
