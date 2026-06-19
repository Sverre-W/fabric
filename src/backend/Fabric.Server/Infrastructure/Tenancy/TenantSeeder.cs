using Fabric.Server.Tenants.Domain;
using Fabric.Server.Tenants.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fabric.Server.Infrastructure.Tenancy;

public sealed class TenantSeeder(TenantsDbContext dbContext, IOptions<TenancyOptions> options)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (options.Value.Mode != TenancyMode.SingleTenant)
            return;

        DefaultTenantOptions defaultTenant = options.Value.DefaultTenant;

        bool exists = await dbContext.Tenants.AnyAsync(tenant => tenant.Id == defaultTenant.Id, cancellationToken);
        if (exists)
            return;

        var tenant = Tenant.Create(defaultTenant.Id, new TenantConfiguration
        {
            Oidc = new OidcSettings
            {
                MetadataUrl = defaultTenant.Oidc.MetadataUrl!,
                ClientId = defaultTenant.Oidc.ClientId!,
                RequireHttpsMetadata = defaultTenant.Oidc.RequireHttpsMetadata
            },
            GraphEmail = defaultTenant.GraphEmail
        });

        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
