using System.Text.Json.Serialization;
using Fabric.Server.Core;
using Fabric.Server.Reception.Contracts;
using Fabric.Server.Reception.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Reception;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ArrivalType?))]
[JsonSerializable(typeof(ListArrivalsRequest))]
[JsonSerializable(typeof(OnboardArrivalRequest))]
[JsonSerializable(typeof(OnboardingStatus?))]
[JsonSerializable(typeof(Page<ArrivalResponse>))]
[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class ReceptionJsonSerializerContext : JsonSerializerContext;
