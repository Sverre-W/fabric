using Fabric.Server.Infrastructure.Tenancy;
using Fabric.Server.Tenants.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Fabric.Server.Tenants.Endpoints;

public static class TenantsEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tenants/settings", GetTenantSettings)
            .AllowAnonymous()
            .WithDescription("Retrieve tenant settings")
            .WithSummary("Retrieve tenant settings")
            .Produces<TenantSettingsResponse>();

        return app;
    }

    private static IResult GetTenantSettings(ITenantContext tenantContext) =>
        Results.Ok(tenantContext.Configuration.ToResponse());
}
