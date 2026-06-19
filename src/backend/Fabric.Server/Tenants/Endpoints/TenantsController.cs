using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Tenants.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fabric.Server.Tenants.Endpoints;

[ApiController]
public sealed class TenantsController
{
    [AllowAnonymous]
    [HttpGet("/api/tenants/settings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TenantSettingsResponse))]
    [EndpointDescription("Retrieve tenant settings")]
    [EndpointSummary("Retrieve tenant settings")]
    public IResult GetTenantSettings([FromServices] ITenantContext tenantContext) =>
        Results.Ok(tenantContext.Configuration.ToResponse());
}
