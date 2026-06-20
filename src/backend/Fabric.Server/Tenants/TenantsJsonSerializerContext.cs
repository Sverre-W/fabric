using System.Text.Json.Serialization;
using Fabric.Server.Tenants.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Tenants;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(TenantSettingsResponse))]
[JsonSerializable(typeof(AdminTenantSettingsResponse))]
[JsonSerializable(typeof(UpdateTenantSettingsRequest))]
internal sealed partial class TenantsJsonSerializerContext : JsonSerializerContext;
