using System.Text.Json.Serialization;
using Fabric.Server.Core;
using Fabric.Server.Employees.Contracts;
using Fabric.Server.Employees.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Employees;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ListEmployeesRequest))]
[JsonSerializable(typeof(CreateEmployeeRequest))]
[JsonSerializable(typeof(UpdateEmployeeWorkDetailsRequest))]
[JsonSerializable(typeof(TransitionEmployeeStatusRequest))]
[JsonSerializable(typeof(ListOrganizationUnitsRequest))]
[JsonSerializable(typeof(CreateOrganizationUnitRequest))]
[JsonSerializable(typeof(UpdateOrganizationUnitRequest))]
[JsonSerializable(typeof(MoveOrganizationUnitRequest))]
[JsonSerializable(typeof(Page<EmployeeResponse>))]
[JsonSerializable(typeof(EmployeeResponse))]
[JsonSerializable(typeof(IdentitySummaryResponse))]
[JsonSerializable(typeof(OrganizationUnitSummaryResponse))]
[JsonSerializable(typeof(Page<OrganizationUnitResponse>))]
[JsonSerializable(typeof(OrganizationUnitResponse))]
[JsonSerializable(typeof(OrganizationUnitResponse[]))]
[JsonSerializable(typeof(EmployeeStatus))]
[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class EmployeesJsonSerializerContext : JsonSerializerContext;
