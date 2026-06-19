using Fabric.Server.Tenants.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Tenants;

public static class TenantServiceCollectionExtensions
{
    public static IServiceCollection SetupTenants(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<TenantsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", TenantsDbContext.Schema));
        });

        return collection;
    }
}
