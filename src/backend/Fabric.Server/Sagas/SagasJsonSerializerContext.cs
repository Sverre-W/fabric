using System.Text.Json.Serialization;
using Fabric.Server.Sagas.Kiosk;
using Fabric.Server.Sagas.VisitorPreOnboarding;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Sagas;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(List<VisitorPreOnboardingSaga>))]
[JsonSerializable(typeof(List<KioskSaga>))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(VisitorPreOnboardingSaga))]
[JsonSerializable(typeof(KioskSaga))]
[JsonSerializable(typeof(VisitorPreOnboardingSagaConfig))]
[JsonSerializable(typeof(VisitorPreOnboardingSagaConfigRequest))]
internal sealed partial class SagasJsonSerializerContext : JsonSerializerContext;
