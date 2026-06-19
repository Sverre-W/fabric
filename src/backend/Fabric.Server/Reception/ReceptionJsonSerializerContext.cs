using System.Text.Json.Serialization;
using Fabric.Server.Core;
using Fabric.Server.Reception.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Reception;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(OnboardArrivalRequest))]
[JsonSerializable(typeof(Page<ArrivalResponse>))]
[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class ReceptionJsonSerializerContext : JsonSerializerContext;
