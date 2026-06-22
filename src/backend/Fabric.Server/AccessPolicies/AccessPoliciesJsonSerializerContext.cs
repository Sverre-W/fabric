using System.Text.Json.Serialization;
using Fabric.Server.AccessPolicies.Contracts;
using Fabric.Server.AccessPolicies.Domain;
using Fabric.Server.Core;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.AccessPolicies;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(AccessLevelResponse))]
[JsonSerializable(typeof(AccessLevelTypeResponse))]
[JsonSerializable(typeof(AccessPolicyChangeResponse))]
[JsonSerializable(typeof(AccessPolicyResponse))]
[JsonSerializable(typeof(AccessRequirementResponse))]
[JsonSerializable(typeof(AddLenelAccessLevelTypeRequest))]
[JsonSerializable(typeof(AddLenelBadgeTypeRequest))]
[JsonSerializable(typeof(AddUnipassAccessLevelTypeRequest))]
[JsonSerializable(typeof(AddUnipassBadgeTypeRequest))]
[JsonSerializable(typeof(BadgeTypeResponse))]
[JsonSerializable(typeof(CreateAccessControlSystemRequest))]
[JsonSerializable(typeof(CreateAccessPolicyRequest))]
[JsonSerializable(typeof(CreateCredentialPolicyRequest))]
[JsonSerializable(typeof(CreateLenelAccessControlSystemRequest))]
[JsonSerializable(typeof(CreateUnipassAccessControlSystemRequest))]
[JsonSerializable(typeof(CredentialRequirementResponse))]
[JsonSerializable(typeof(CredentialResponse))]
[JsonSerializable(typeof(IdentityMappingResponse))]
[JsonSerializable(typeof(IssuedResourceResponse))]
[JsonSerializable(typeof(LenelAccessControlSystemResponse))]
[JsonSerializable(typeof(LenelBadgeTypeResponse))]
[JsonSerializable(typeof(LenelAccessLevelTypeResponse))]
[JsonSerializable(typeof(LenelMetadata))]
[JsonSerializable(typeof(Page<AccessControlSystemResponse>))]
[JsonSerializable(typeof(Page<AccessPolicyResponse>))]
[JsonSerializable(typeof(Page<IdentityMappingResponse>))]
[JsonSerializable(typeof(PolicyRequirementResponse))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(SubjectSystemAccessStateResponse))]
[JsonSerializable(typeof(SystemMetadata))]
[JsonSerializable(typeof(UnipassAccessControlSystemResponse))]
[JsonSerializable(typeof(UnipassAccessLevelTypeResponse))]
[JsonSerializable(typeof(UnipassBadgeTypeResponse))]
[JsonSerializable(typeof(UnipassMetadata))]
[JsonSerializable(typeof(UpdateLenelConfigRequest))]
[JsonSerializable(typeof(UpdateUnipassConfigRequest))]
internal sealed partial class AccessPoliciesJsonSerializerContext : JsonSerializerContext;
