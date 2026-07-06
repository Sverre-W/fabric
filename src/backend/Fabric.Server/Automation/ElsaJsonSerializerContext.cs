using System.Text.Json.Serialization;

namespace Fabric.Server.Automation;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(string[]))]
internal sealed partial class ElsaJsonSerializerContext : JsonSerializerContext;
