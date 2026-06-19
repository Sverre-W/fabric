using System.Text.Json.Serialization;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Sagas;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<VisitorPreOnboardingSaga>))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(VisitorPreOnboardingSaga))]
internal sealed partial class SagasJsonSerializerContext : JsonSerializerContext;
