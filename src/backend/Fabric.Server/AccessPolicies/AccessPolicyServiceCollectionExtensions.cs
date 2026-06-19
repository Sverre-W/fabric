using Fabric.Server.AccessPolicies.Application;
using Fabric.Server.AccessPolicies.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.AccessPolicies;

public static class AccessPolicyServiceCollectionExtensions
{
    public static IServiceCollection SetupAccessPolicies(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<AccessPoliciesDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", AccessPoliciesDbContext.Schema));
        });

        collection.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Add(AccessPoliciesJsonSerializerContext.Default));

        collection.AddScoped<AccessPolicyService>();
        collection.AddScoped<AccessControlSystemService>();

        return collection;
    }
}
