using Fabric.Server.Locations.Application;
using Fabric.Server.Locations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fabric.Server.Locations;

public static class LocationServiceCollectionExtensions
{
    public static IServiceCollection SetupLocations(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddDbContext<LocationsDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Database"),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", LocationsDbContext.Schema));
        });

        collection.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Add(LocationsJsonSerializerContext.Default));

        collection.AddScoped<LocationService>();
        return collection;
    }
}
