using System.Text.Json.Serialization;
using Fabric.Server.Core;
using Fabric.Server.CredentialManagement.Contracts;
using Fabric.Server.CredentialManagement.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.CredentialManagement;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ListCredentialTypesRequest))]
[JsonSerializable(typeof(ListCredentialsRequest))]
[JsonSerializable(typeof(ListCredentialReservationsRequest))]
[JsonSerializable(typeof(CreateCredentialTypeRequest))]
[JsonSerializable(typeof(UpdateCredentialTypeRequest))]
[JsonSerializable(typeof(CreateCredentialTypeTargetRequest))]
[JsonSerializable(typeof(UpdateCredentialTypeTargetRequest))]
[JsonSerializable(typeof(CreateCredentialReservationRequest))]
[JsonSerializable(typeof(IssueCredentialRequest))]
[JsonSerializable(typeof(Page<CredentialTypeResponse>))]
[JsonSerializable(typeof(Page<CredentialResponse>))]
[JsonSerializable(typeof(Page<CredentialReservationResponse>))]
[JsonSerializable(typeof(CredentialTypeResponse))]
[JsonSerializable(typeof(CredentialTypeTargetResponse))]
[JsonSerializable(typeof(CredentialTypeTargetResponse[]))]
[JsonSerializable(typeof(CredentialReservationResponse))]
[JsonSerializable(typeof(CredentialResponse))]
[JsonSerializable(typeof(CredentialProvisioningTransactionResponse))]
[JsonSerializable(typeof(CredentialProvisioningTransactionResponse[]))]
[JsonSerializable(typeof(CredentialTechnology))]
[JsonSerializable(typeof(CredentialTypeStatus))]
[JsonSerializable(typeof(CredentialCapacityState))]
[JsonSerializable(typeof(CredentialDurationKind))]
[JsonSerializable(typeof(CredentialStatus))]
[JsonSerializable(typeof(CredentialReservationStatus))]
[JsonSerializable(typeof(ProvisioningTiming))]
[JsonSerializable(typeof(CredentialProvisioningStatus))]
[JsonSerializable(typeof(CredentialPurpose))]
[JsonSerializable(typeof(CredentialSourceKind))]
[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class CredentialManagementJsonSerializerContext : JsonSerializerContext;
