using System.Text.Json.Serialization;
using FastEndpoints;

namespace Fabric.Server.Automation;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(EmptyResponse))]
[JsonSerializable(typeof(EmptyRequest))]
[JsonSerializable(typeof(Type))]
[JsonSerializable(typeof(Version))]
internal sealed partial class ElsaJsonSerializerContext : JsonSerializerContext;
